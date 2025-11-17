import React, { useEffect, useState } from "react";

export default function CommandList() {
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
                <p> оманды не найдены</p>
            ) : (
                <ul>
                    {commands.map((cmd) => (
                        <li key={cmd.id}>
                            {cmd.name} Ч {cmd.actionType} Ч {cmd.actionData}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
