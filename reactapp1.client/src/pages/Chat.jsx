import { useState } from 'react';
import './Chat.css';

function Chat() {
    const [messages, setMessages] = useState([
        { role: 'system', content: 'Welcome! Ask your question about de Tweede Kamer.' }
    ]);
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
            const response = await fetch('/api/LmStudio', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ messages: newMessages.filter(m => m.role !== 'system') })
            });
            const data = await response.json();
            setMessages(data.messages);
        } catch (err) {
            setMessages([...newMessages, { role: 'system', content: 'Error: Could not get response.' }]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="chat-page">
            <div className="chatbox">
                <div className="chat-messages">
                    {messages.map((msg, idx) => (
                        <div key={idx} className={`chat-message ${msg.role}`}>
                            <span>{msg.role === 'user' ? 'You' : msg.role === 'assistant' ? 'AI' : 'System'}: </span>
                            {msg.content}
                        </div>
                    ))}
                    {loading && <div className="chat-message assistant">AI is typing...</div>}
                </div>
                <form className="chat-input-row" onSubmit={sendMessage}>
                    <input
                        type="text"
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        placeholder="Type your message..."
                        disabled={loading}
                        autoFocus
                    />
                    <button type="submit" disabled={loading || !input.trim()}>Send</button>
                </form>
            </div>
        </div>
    );
}

export default Chat;