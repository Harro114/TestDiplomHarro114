import axios, {AxiosError} from 'axios';
import Cookies from 'js-cookie';
import {Navigate, useNavigate} from 'react-router-dom';


const API_BASE_URL = '/api';


const http = axios.create({
    baseURL: 'https://localhost:7200',
    headers: {
        'Content-Type': 'application/json',
    },
});


http.interceptors.request.use((config) => {
    const token = Cookies.get('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

http.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
        if (error.response?.status === 401) {
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export const login = async (credentials: { Username: string; Password: string }): Promise<void> => {
    const response = await http.post<{ token: string }>('/api/Auth/login', credentials);
    if (response.status === 409){
        window.location.href = '/blocked';
        return;
    }
    const {token} = response.data;
    if (token) {
        Cookies.set('authToken', token, {secure: true, sameSite: 'Strict'});
    }
};


export const getProfile = async (): Promise<{ id: string, name: string, lastName: string, firstName: string }> => {
    const response = await http.get<{ id: string, name: string, lastName: string, firstName: string }>('/api/profile');
    return response.data;
};

export default http;