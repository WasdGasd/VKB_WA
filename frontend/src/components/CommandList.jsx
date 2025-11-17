import React, { useEffect, useState } from "react";

export const CommandList = () => {
    const [commands, setCommands] = useState([]);

    useEffect(() => {
        fetch("http://localhost:5000/api/commands") // URL твоего API
            .then((res) => res.json())
            .then((data) => setCommands(data))
            .catch((err) => console.error(err));
    }, []);

    return (
        <div>
            {commands.length === 0 ? (
                <p>Команды не найдены</p>
            ) : (
                <ul>
                    {commands.map((cmd) => (
                        <li key={cmd.id}>
                            {cmd.name} � {cmd.actionType} � {cmd.actionData}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
