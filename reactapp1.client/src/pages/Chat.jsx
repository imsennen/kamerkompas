import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './Chat.css';

// Formatter to utilize the \n and ** in the AI response
function formatMessage(content) {
    let formatted = content.replace(/\*\*(.+?)\*\*/g, '<b>$1</b>');
    formatted = formatted.replace(/\n/g, '<br />');
    return formatted;
}

function Chat() {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);
    const messagesEndRef = useRef(null);
    const inputRef = useRef(null); 

    //scroll to bottom when messages or loading changes
    useEffect(() => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages, loading]);

    //focus input when loading finishes
    useEffect(() => {
        if (!loading && inputRef.current) {
            inputRef.current.focus();
        }
    }, [loading]);

    const sendMessage = async (e) => {
        e.preventDefault();
        if (!input.trim()) return; 

        //User's message gets added
        const newMessages = [...messages, { role: 'user', content: input }]; //
        setMessages(newMessages);
        setInput('');
        setLoading(true);

        try {
            //send messages to the api
            const response = await fetch('https://localhost:7059/api/gemini', {         //HIERZO:
            //const response = await fetch('https://localhost:7059/api/LmStudio', {     //Wissel deze 2 lijnen om als u wilt wisselen van de GeminiAPI of de LMStudio.
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ messages: newMessages })
            });
            const data = await response.json();
            if (data.messages) {
                setMessages(data.messages); //update messages

            } else {
                throw new Error('No messages returned from API');
            }
        } catch (err) {
            //error handling
            console.log(err)
            setMessages([...newMessages, { role: 'system', content: 'Error: Kon geen reactie krijgen.' }]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="chat-page">
            <div className="chatbox">
                <div className="chat-messages">
                    {/* System message */}
                    <div className="chat-message system">
                        <span>Systeem: </span>
                        Welkom! Stel je vraag over de Tweede Kamer. Lees hier de{' '}
                        <Link to="/disclaimer">disclaimer</Link>.
                    </div>
                    { /* chat window */ }
                    {messages.map((msg, idx) => (
                        <div key={idx} className={`chat-message ${msg.role}`}>
                            <span>{msg.role === 'user' ? 'Jij' : msg.role === 'assistant' ? 'AI' : 'System'}: </span>
                            <span
                                dangerouslySetInnerHTML={{ __html: formatMessage(msg.content) }}
                            />
                        </div>
                    ))}
                    { /* loading message */}
                    {loading && <div className="chat-message assistant">AI: Laat me even denken...</div>}
                    { /* scrolling */}
                    <div ref={messagesEndRef} />
                </div>
                { /* Input */}
                <form className="chat-input-row" onSubmit={sendMessage}>
                    <input
                        type="text"
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        placeholder="Typ uw bericht..."
                        disabled={loading}
                        ref={inputRef}
                        autoFocus
                    />
                    <button type="submit" disabled={loading || !input.trim()}>Verstuur</button>
                </form>
            </div>
        </div>
    );
}

export default Chat;