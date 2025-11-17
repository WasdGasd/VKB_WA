import React from "react";
import { Link } from "react-router-dom";

export default function Dashboard() {
    return (
        <div>
            <h1>Панель управления ботом</h1>
            <nav>
                <Link to="/commands">Команды</Link>
            </nav>
            <p>Статус бота и действия можно будет добавить здесь</p>
        </div>
    );
}
