import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useLocation } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { API_BASE_URL } from '../../constant/Constants';
import './Chat.css';

const Chat = () => {
    const location = useLocation();
    const {recipientId, bookingId, recipientName } = location.state || {};

    const currentUserId = localStorage.getItem('userId');
    const token = localStorage.getItem('userToken');

    const connectionRef = useRef(null);
    const messagesEndRef = useRef(null);

    const [messages, setMessages] = useState([]);
    const [messageInput, setMessageInput] = useState('');
    const [isConnected, setIsConnected] = useState(false);

    
    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    
    useEffect(() => {
        if (!token) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE_URL}/chatHub`, { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => {
                console.log('✅ Connected to Chat Hub!');
                setIsConnected(true);

                
                connection.on('ReceiveMessage', (senderId, senderUsername, message) => {
                    if (message) {
                        setMessages(prev => [...prev, { senderId, senderUsername, message }]);
                    }
                });
            })
            .catch(err => {
                console.error('❌ Chat Hub connection failed:', err);
                setIsConnected(false);
            });

        connectionRef.current = connection;

       
        return () => {
            connection.off('ReceiveMessage');
            connection.stop().catch(() => {});
        };
    }, [token]);

   
    const sendMessage = useCallback(async () => {
        const connection = connectionRef.current;
        const trimmedMessage = messageInput.trim();

        if (!connection || !isConnected) {
            console.warn('⚠️ Connection not ready.');
            return;
        }

        if (!trimmedMessage || !recipientId) return;

        try {
            await connection.invoke('SendPrivateMessage', recipientId, trimmedMessage);
            setMessageInput('');
        } catch (err) {
            console.error('❌ Failed to send message:', err);
        }
    }, [messageInput, recipientId, isConnected]);

    return (
        <div className="chat-container">
            <h2 className="chat-header">
                Chat with {recipientName} {isConnected ? '🟢' : '🔴'}
            </h2>

            <div className="chat-messages">
                {messages.map((msg, index) => (
                    <div
                        key={index}
                        className={`chat-message ${msg.senderId === currentUserId ? 'chat-message-me' : 'chat-message-other'}`}
                    >
                     {msg.message}
                    </div>
                ))}
                <div ref={messagesEndRef} />
            </div>

            <div className="chat-input-container">
                <input
                    type="text"
                    placeholder="Type your message..."
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && sendMessage()}
                    className="chat-input"
                    disabled={!isConnected}
                />
                <button
                    onClick={sendMessage}
                    className="chat-send-button"
                    disabled={!isConnected || !messageInput.trim()}
                >
                    Send
                </button>
            </div>
        </div>
    );
};

export default Chat;
