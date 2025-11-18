import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import { NotificationProvider } from "./contexts/NotificationContext";
import { ThemeProvider } from "./contexts/ThemeContext";
import Sidebar from "./components/Sidebar";
import Dashboard from "./pages/Dashboard";
import CommandsPage from "./pages/CommandsPage";
import LogsPage from "./pages/LogsPage";
import Login from "./pages/Login";

const ProtectedRoute = ({ children }) => {
    const { isAuthenticated, loading } = useAuth();

    if (loading) {
        return <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh'
        }}>Loading...</div>;
    }

    return isAuthenticated ? children : <Navigate to="/login" />;
};

const AppLayout = () => {
    return (
        <div style={{ display: 'flex' }}>
            <Sidebar />
            <div style={{
                marginLeft: '250px',
                padding: '20px',
                width: 'calc(100% - 250px)',
                minHeight: '100vh',
                background: 'var(--bg-secondary)'
            }}>
                <Routes>
                    <Route path="/" element={
                        <ProtectedRoute>
                            <Dashboard />
                        </ProtectedRoute>
                    } />
                    <Route path="/commands" element={
                        <ProtectedRoute>
                            <CommandsPage />
                        </ProtectedRoute>
                    } />
                    <Route path="/logs" element={
                        <ProtectedRoute>
                            <LogsPage />
                        </ProtectedRoute>
                    } />
                </Routes>
            </div>
        </div>
    );
};

function App() {
    return (
        <ThemeProvider>
            <NotificationProvider>
                <AuthProvider>
                    <Router>
                        <Routes>
                            <Route path="/login" element={<Login />} />
                            <Route path="*" element={<AppLayout />} />
                        </Routes>
                    </Router>
                </AuthProvider>
            </NotificationProvider>
        </ThemeProvider>
    );
}

export default App;