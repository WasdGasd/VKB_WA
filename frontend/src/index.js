import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Dashboard from "./pages/Dashboard";
import CommandsPage from "./pages/CommandsPage";

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/commands" element={<CommandsPage />} />
            </Routes>
        </Router>
    );
}

export default App;
