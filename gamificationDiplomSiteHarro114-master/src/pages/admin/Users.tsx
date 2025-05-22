import React, {useEffect, useState} from 'react';
import {Table, Modal, notification, Switch, Select, Button} from 'antd';
import type {ColumnsType} from 'antd/es/table';
import http from "../../services/authService";

const {Option} = Select;

interface User {
    id: number;
    username: string;
    userLastName: string;
    userFirstName: string;
    isBlocked: boolean;
    roleName: string;
}

interface BalanceHistory {
    id: number;
    createdAt: string;
    discription: string;
    value: number;
    currentBalance: number;
}

interface Role {
    id: number;
    name: string;
}

const Users: React.FC = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(false);
    const [roles, setRoles] = useState<Role[]>([]);
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [balanceHistory, setBalanceHistory] = useState<BalanceHistory[]>([]);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [api, contextHolder] = notification.useNotification();

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const response = await http.get('/api/Admin/getAllUsers');
            setUsers(response.data);
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка загрузки пользователей',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        } finally {
            setLoading(false);
        }
    };

    const fetchRoles = async () => {
        try {
            const response = await http.get('/api/Admin/getRoles');
            setRoles(response.data);
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка загрузки ролей',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        }
    };

    const fetchUserBalanceHistory = async (accountId: number) => {
        try {
            const response = await http.get(`/api/Admin/getUserHistory`, {
                params: {accountId},
            });
            console.log(response.data);
            setBalanceHistory(response.data);
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка загрузки истории баланса',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        }
    };

    const handleRoleChange = async (userId: number, roleId: number) => {
        try {
            await http.post('/api/Admin/changeRole', {AccountId: userId, RoleId: roleId});
            notification.success({
                message: 'Роль изменена',
                description: `Роль пользователя успешно изменена.`,
            });
            fetchUsers();
        } catch (error: any) {
            if (error.response?.status === 400) {
                notification.error({
                    message: 'Ошибка изменения роли',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        }
    };

    const handleBlockToggle = async (userId: number, isBlocked: boolean) => {
        try {
            await http.post('/api/Admin/blockedUser', {UserId: userId, IsBlocked: isBlocked});
            notification.success({
                message: 'Статус блокировки изменен',
                description: `Пользователь ${isBlocked ? 'заблокирован' : 'разблокирован'}.`,
            });
            fetchUsers();
        } catch (error: any) {
            if (error.response?.status === 400) {
                notification.error({
                    message: 'Ошибка изменения статуса блокировки',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        }
    };

    const handleUserClick = (user: User) => {
        setSelectedUser(user);
        fetchUserBalanceHistory(user.id);
        setIsModalVisible(true);
    };

    const closeModal = () => {
        setSelectedUser(null);
        setBalanceHistory([]);
        setIsModalVisible(false);
    };

    useEffect(() => {
        fetchUsers();
        fetchRoles();
    }, []);

    const columns: ColumnsType<User> = [
        {
            title: 'ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Логин',
            dataIndex: 'username',
            key: 'username',
            render: (text, record) => (
                <Button type="link" onClick={() => handleUserClick(record)}>
                    {text}
                </Button>
            ),
        },
        {
            title: 'Фамилия',
            dataIndex: 'userLastName',
            key: 'userLastName',
        },
        {
            title: 'Имя',
            dataIndex: 'userFirstName',
            key: 'userFirstName',
        },
        {
            title: 'Статус блокировки',
            dataIndex: 'isBlocked',
            key: 'isBlocked',
            render: (value, record) => (
                <Switch
                    checked={value}
                    onChange={(checked) => handleBlockToggle(record.id, checked)}
                />
            ),
        },
        {
            title: 'Роль',
            dataIndex: 'roleName',
            key: 'roleName',
            render: (value, record) => (
                <Select
                    defaultValue={roles.find((role) => role.name === value)?.id}
                    onChange={(roleId) => handleRoleChange(record.id, roleId)}
                    style={{width: 150}}
                >
                    {roles.map((role) => (
                        <Option key={role.id} value={role.id}>
                            {role.name}
                        </Option>
                    ))}
                </Select>
            ),
        },
    ];

    const balanceColumns: ColumnsType<BalanceHistory> = [
        {
            title: 'Дата',
            dataIndex: 'createdAt',
            key: 'createdAt',
        },
        {
            title: 'Описание',
            dataIndex: 'discription',
            key: 'discription',
        },
        {
            title: 'Изменение баланса',
            dataIndex: 'value',
            key: 'value',
        },
        {
            title: 'Текущий баланс',
            dataIndex: 'currentBalance',
            key: 'currentBalance',
        },
    ];

    return (
        <>
            {contextHolder}
            <Table
                columns={columns}
                dataSource={users}
                loading={loading}
                rowKey="id"
            />

            <Modal
                title={`История баланса: ${selectedUser?.username}`}
                visible={isModalVisible}
                onCancel={closeModal}
                footer={null}
                width="90%"
            >
                <Table
                    columns={balanceColumns}
                    dataSource={balanceHistory}
                    rowKey="id"
                    pagination={false}
                />
            </Modal>
        </>
    );
};

export default Users;