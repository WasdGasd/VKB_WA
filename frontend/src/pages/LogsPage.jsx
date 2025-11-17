import React, { useEffect, useState } from 'react';

export default function LogsPage() {
    const [logs, setLogs] = useState([]);

    useEffect(() => {
        // TODO: fetch logs from API
    }, []);

    return (
        <div>
            <h2>Bot Logs</h2>
            <ul>
                {logs.map((log, i) => <li key={i}>{log.message}</li>)}
            </ul>
        </div>
    );
}
