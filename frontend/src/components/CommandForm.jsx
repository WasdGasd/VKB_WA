import React, { useState, useEffect } from 'react';

export default function CommandForm({ command, onSave }) {
    const [name, setName] = useState('');
    const [actionType, setActionType] = useState('SendText');
    const [actionData, setActionData] = useState('');

    useEffect(() => {
        if (command) {
            setName(command.name);
            setActionType(command.actionType);
            setActionData(command.actionData);
        } else {
            setName(''); setActionType('SendText'); setActionData('');
        }
    }, [command]);

    const handleSubmit = e => {
        e.preventDefault();
        onSave({ id: command?.id, name, actionType, actionData });
    };

    return (
        <form onSubmit={handleSubmit}>
            <input
                placeholder="Название команды"
                value={name}
                onChange={e => setName(e.target.value)}
                required
            />
            <select value={actionType} onChange={e => setActionType(e.target.value)}>
                <option value="SendText">Отправить текст</option>
                <option value="RunMethod">Запустить метод</option>
                <option value="CallHttp">Вызвать HTTP</option>
                <option value="RunScript">Запустить скрипт</option>
            </select>
            <input
                placeholder="Данные для действия"
                value={actionData}
                onChange={e => setActionData(e.target.value)}
                required
            />
            <button type="submit">
                {command ? 'Обновить' : 'Сохранить'}
            </button>
        </form>
    );
}
