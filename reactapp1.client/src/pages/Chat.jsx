import { useState } from 'react';
import { Link } from 'react-router-dom';
import './Chat.css';

// Helper to format message content
function formatMessage(content) {
    let formatted = content.replace(/\*\*(.+?)\*\*/g, '<b>$1</b>');
    formatted = formatted.replace(/\n/g, '<br />');
    return formatted;
}

function Chat() {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);

    const sendMessage = async (e) => {
        e.preventDefault();
        if (!input.trim()) return;

        const newMessages = [...messages, { role: 'user', content: input }];
        setMessages(newMessages);
        setInput('');
        setLoading(true);

        try {
            const response = await fetch('https://localhost:7059/api/LmStudio', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ messages: newMessages })
            });
            const data = await response.json();
            setMessages(data.messages);
        } catch (err) {
            setMessages([...newMessages, { role: 'system', content: 'Error: CKon geen reactie krijgen.' }]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="chat-page">
            <div className="chatbox">
                <div className="chat-messages">
                    {/* Always show system message at the top */}
                    <div className="chat-message system">
                        <span>System: </span>
                        Welkom! Stel je vraag over de Tweede Kamer. Lees hier de{' '}
                        <Link to="/disclaimer">disclaimer</Link>.
                    </div>
                    {messages.map((msg, idx) => (
                        <div key={idx} className={`chat-message ${msg.role}`}>
                            <span>{msg.role === 'user' ? 'Jij' : msg.role === 'assistant' ? 'AI' : 'System'}: </span>
                            <span
                                dangerouslySetInnerHTML={{ __html: formatMessage(msg.content) }}
                            />
                        </div>
                    ))}
                    {loading && <div className="chat-message assistant">AI: Laat me even denken...</div>}
                </div>
                <form className="chat-input-row" onSubmit={sendMessage}>
                    <input
                        type="text"
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        placeholder="Typ uw bericht..."
                        disabled={loading}
                        autoFocus
                    />
                    <button type="submit" disabled={loading || !input.trim()}>Verstuur</button>
                </form>
            </div>
        </div>
    );
}

export default Chat;