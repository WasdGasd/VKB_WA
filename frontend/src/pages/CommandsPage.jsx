import React from "react";
import CommandForm from "../components/CommandForm";
import { CommandList } from "../components/CommandList";

export default function CommandsPage() {
    const handleSave = (commandData) => {
        console.log('Сохранение команды:', commandData);
        // TODO: Отправка на сервер
    };

    return (
        <div style={{ padding: '20px' }}>
            <h1>Управление командами бота</h1>

            <h2>Добавить новую команду</h2>
            <CommandForm onSave={handleSave} />

            <h2>Список команд</h2>
            <CommandList />
        </div>
    );
}
