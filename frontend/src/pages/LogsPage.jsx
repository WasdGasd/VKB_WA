import React from 'react';

const LogsPage = () => {
    return (
        <div>
            <h1 style={{ color: '#2c3e50', marginBottom: '20px' }}>Logs</h1>
            <div style={{
                background: '#fff',
                padding: '20px',
                borderRadius: '8px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                border: '1px solid #e0e0e0'
            }}>
                <p>Logs monitoring will be implemented here...</p>
                <div style={{
                    background: '#f8f9fa',
                    padding: '15px',
                    borderRadius: '4px',
                    fontFamily: 'monospace',
                    fontSize: '12px'
                }}>
                    [INFO] 2024-01-15 10:30:15 - Bot started successfully<br />
                    [INFO] 2024-01-15 10:31:22 - Received message from user 123<br />
                    [ERROR] 2024-01-15 10:32:05 - Command execution failed
                </div>
            </div>
        </div>
    );
};

export default LogsPage;