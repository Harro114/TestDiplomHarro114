import React, {createContext, useContext, useState, useEffect} from 'react';
import Cookies from 'js-cookie';
import {getProfile} from '../services/authService';
import {useLocation, useNavigate} from 'react-router-dom';

interface AuthContextType {
    accountId: string | null;
    token: string | null;
    login: (id: string, token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({children}) => {
    const [accountId, setAccountId] = useState<string | null>(null);
    const [token, setToken] = useState<string | null>(Cookies.get('authToken') || null);

    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        const fetchProfile = async () => {
            if (token && location.pathname !== '/login') {
                try {
                    const profile = await getProfile();
                    setAccountId(profile.id);
                } catch (err) {
                    console.error('Ошибка получения профиля:', err);
                    logout();
                    navigate('/login');
                }
            }
        };

        fetchProfile();
    }, [token, location.pathname]);

    const login = (id: string, token: string) => {
        setAccountId(id);
        setToken(token);
        Cookies.set('authToken', token, {secure: true, sameSite: 'Strict'});
    };

    const logout = () => {
        setAccountId(null);
        setToken(null);
        Cookies.remove('authToken');
    };

    return (
        <AuthContext.Provider value={{accountId, token, login, logout}}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth должен использоваться внутри AuthProvider');
    }
    return context;
};