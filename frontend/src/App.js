import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Sidebar from "./components/Sidebar";
import Dashboard from "./pages/Dashboard";
import CommandsPage from "./pages/CommandsPage";
import LogsPage from "./pages/LogsPage";

function App() {
    return (
        <Router>
            <div style={{ display: 'flex' }}>
                <Sidebar />
                <div style={{
                    marginLeft: '250px',
                    padding: '20px',
                    width: 'calc(100% - 250px)'
                }}>
                    <Routes>
                        <Route path="/" element={<Dashboard />} />
                        <Route path="/commands" element={<CommandsPage />} />
                        <Route path="/logs" element={<LogsPage />} />
                    </Routes>
                </div>
            </div>
        </Router>
    );
}

export default App;