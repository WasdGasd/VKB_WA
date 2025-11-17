import React from "react";
import { Link } from "react-router-dom";

export default function Dashboard() {
    return (
        <div>
            <h1>Панель управления ботом</h1>
            <nav>
                <Link to="/commands">Команды</Link>
            </nav>
            <p>Здесь можно будет отображать статус бота и действия</p>
        </div>
    );
}
