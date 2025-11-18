import React from 'react';

export const StatCard = ({ title, value }) => (
    <div style={{
        background: '#fff',
        padding: '20px',
        borderRadius: '8px',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
        border: '1px solid #e0e0e0',
        textAlign: 'center',
        minHeight: '100px',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center'
    }}>
        <h3 style={{
            margin: '0 0 10px 0',
            fontSize: '28px',
            fontWeight: 'bold',
            color: '#2c3e50'
        }}>
            {value}
        </h3>
        <p style={{
            margin: '0',
            color: '#7f8c8d',
            fontSize: '14px',
            fontWeight: '500'
        }}>
            {title}
        </p>
    </div>
);