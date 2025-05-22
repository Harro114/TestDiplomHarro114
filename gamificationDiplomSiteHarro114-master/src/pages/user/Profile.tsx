import http, {getProfile} from "../../services/authService";
import {useEffect, useState} from "react";
import {Table} from "antd";

interface UserProfile {
    id: string;
    name: string;
    lastName: string;
    firstName: string;
}

export const expUser = async (): Promise<
    Array<{ value: number; discription: string; createdAt: string }>
> => {
    const response = await http.get('/api/profile/expHistory');
    return response.data.data || [];
};

const Profile = () => {
    const [user, setUser] = useState<UserProfile | null>(null);
    const [expHistory, setExp] = useState<
        Array<{ value: number; discription: string; createdAt: string }>
    >([]);

    const columns = [
        {
            title: 'Значение',
            dataIndex: 'value',
            key: 'value',
        },
        {
            title: 'Описание',
            dataIndex: 'discription',
            key: 'discription',
        },
        {
            title: 'Дата',
            dataIndex: 'createdAt',
            key: 'createdAt',
        },
    ];

    useEffect(() => {
        const fetchUserProfile = async () => {
            try {
                const profile = await getProfile();
                setUser(profile);
            } catch (err) {
                console.error("Ошибка загрузки профиля:", err);
            }
        };
        fetchUserProfile();
    }, []);

    useEffect(() => {
        const fetchExpHistory = async () => {
            try {
                const rawData = await expUser();
                const formattedData = rawData.map(item => ({
                    ...item,
                    createdAt: new Date(item.createdAt).toLocaleString('ru-RU'),
                }));
                setExp(formattedData);
            } catch (err) {
                console.error("Ошибка загрузки истории баланса:", err);
                setExp([]);
            }
        };
        fetchExpHistory();
    }, []);

    return (
        <div>
            <h1>{user?.name}</h1>
            <div style={{alignItems: "center"}}>
                <p>История баланса</p>
                <Table dataSource={expHistory} columns={columns} rowKey="createdAt"/>
            </div>
        </div>
    );
};

export default Profile;