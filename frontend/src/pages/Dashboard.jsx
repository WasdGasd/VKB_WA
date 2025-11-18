import React, { useState, useEffect } from 'react';
import { StatCard } from '../components/StatCard';
import { getStats, getBotStatus } from '../api'; // Измени путь с './api' на '../api'

const Dashboard = () => {
    const [stats, setStats] = useState({});
    const [botStatus, setBotStatus] = useState('loading');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const [statsData, statusData] = await Promise.all([
                getStats(),
                getBotStatus()
            ]);
            setStats(statsData);
            setBotStatus(statusData.status);
        } catch (error) {
            console.error('Error loading data:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return <div style={{ padding: '20px' }}>Loading...</div>;
    }

    return (
        <div style={{
            padding: '20px',
            maxWidth: '1200px',
            margin: '0 auto',
            fontFamily: 'Arial, sans-serif'
        }}>
            <h1 style={{
                marginBottom: '30px',
                color: '#2c3e50',
                borderBottom: '2px solid #ecf0f1',
                paddingBottom: '10px'
            }}>
                Bot Statistics
            </h1>

            <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
                gap: '20px',
                marginBottom: '40px'
            }}>
                <StatCard
                    title="Active Users"
                    value={stats.activeUsers || '0'}
                />
                <StatCard
                    title="Commands Executed"
                    value={stats.commandsExecuted || '0'}
                />
                <StatCard
                    title="Errors Today"
                    value={stats.errorsToday || '0'}
                />
                <StatCard
                    title="Bot Status"
                    value={botStatus}
                    style={{
                        color: botStatus === 'online' ? '#27ae60' :
                            botStatus === 'offline' ? '#e74c3c' : '#f39c12'
                    }}
                />
            </div>

            <div style={{
                background: '#fff',
                padding: '20px',
                borderRadius: '8px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                border: '1px solid #e0e0e0'
            }}>
                <h2 style={{ color: '#2c3e50', marginTop: '0' }}>
                    Recent Activity
                </h2>
                <p style={{ color: '#7f8c8d', fontStyle: 'italic' }}>
                    Real-time activity feed will be implemented here...
                </p>
            </div>
        </div>
    );
};

export default Dashboard;