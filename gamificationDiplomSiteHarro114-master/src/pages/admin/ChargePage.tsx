import React, {useEffect, useState} from 'react';
import {Form, Input, Select, Button, notification, Switch} from 'antd';
import http from "../../services/authService";

const {Option} = Select;

interface User {
    id: number;
    username: string;
}

interface Discount {
    id: number;
    name: string;
}

const ChargePage: React.FC = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [discounts, setDiscounts] = useState<Discount[]>([]);
    const [isDiscountMode, setIsDiscountMode] = useState(true);
    const [selectedUser, setSelectedUser] = useState<number | null>(null);
    const [selectedDiscount, setSelectedDiscount] = useState<number | null>(null);
    const [currencyValue, setCurrencyValue] = useState<number | null>(null);
    const [description, setDescription] = useState<string>('');
    const [loading, setLoading] = useState(false);

    const [api, contextHolder] = notification.useNotification();

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const response = await http.get('/api/Admin/getAllUsers');
                setUsers(response.data);
            } catch (error: any) {
                if (error.response?.status === 400) {
                    api.error({
                        message: 'Ошибка загрузки пользователей',
                        description: error.response.data?.message || 'Ошибка сервера',
                    });
                }
            }
        };

        const fetchDiscounts = async () => {
            try {
                const response = await http.get('/api/Admin/getAllDiscounts');
                setDiscounts(response.data);
            } catch (error: any) {
                if (error.response?.status === 400) {
                    api.error({
                        message: 'Ошибка загрузки скидок',
                        description: error.response.data?.message || 'Ошибка сервера',
                    });
                }
            }
        };

        fetchUsers();
        fetchDiscounts();
    }, [api]);

    const handleSubmission = async () => {
        if (!selectedUser) {
            api.error({message: 'Ошибка', description: 'Выберите пользователя.'});
            return;
        }

        setLoading(true);

        try {
            if (isDiscountMode) {
                if (!selectedDiscount) {
                    api.error({message: 'Ошибка', description: 'Выберите скидку.'});
                    return;
                }
                await http.post('/api/Admin/chargeDiscount', {
                    AccountId: selectedUser,
                    DiscountId: selectedDiscount,
                });
                api.success({
                    message: 'Успех',
                    description: 'Скидка успешно начислена.',
                });
            } else {
                if (!currencyValue || currencyValue <= 0) {
                    api.error({message: 'Ошибка', description: 'Введите количество валюты.'});
                    return;
                }
                await http.post('/api/Admin/chargeExp', {
                    AccountId: selectedUser,
                    Value: currencyValue,
                    Discription: description || 'Без описания',
                });
                api.success({
                    message: 'Успех',
                    description: 'Валюта успешно начислена.',
                });
            }
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка',
                    description: error.response.data?.message || 'Ошибка сервера',
                });
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{padding: '20px'}}>
            {contextHolder}
            <h2>Начисление скидок и валюты</h2>
            <Form layout="vertical">
                <Form.Item label="Режим">
                    <Switch
                        checked={isDiscountMode}
                        onChange={(checked) => setIsDiscountMode(checked)}
                        checkedChildren="Скидка"
                        unCheckedChildren="Валюта"
                    />
                </Form.Item>

                <Form.Item label="Пользователь" required>
                    <Select
                        placeholder="Выберите пользователя"
                        onChange={(value) => setSelectedUser(value)}
                        allowClear
                    >
                        {users.map((user) => (
                            <Option key={user.id} value={user.id}>
                                {user.username}
                            </Option>
                        ))}
                    </Select>
                </Form.Item>

                {isDiscountMode ? (
                    <Form.Item label="Скидка" required>
                        <Select
                            placeholder="Выберите скидку"
                            onChange={(value) => setSelectedDiscount(value)}
                            allowClear
                        >
                            {discounts.map((discount) => (
                                <Option key={discount.id} value={discount.id}>
                                    {discount.name}
                                </Option>
                            ))}
                        </Select>
                    </Form.Item>
                ) : (
                    <>
                        <Form.Item label="Количество валюты" required>
                            <Input
                                type="number"
                                placeholder="Введите количество"
                                min={1}
                                onChange={(e) => setCurrencyValue(Number(e.target.value))}
                            />
                        </Form.Item>
                        <Form.Item label="Описание">
                            <Input.TextArea
                                placeholder="Введите описание"
                                onChange={(e) => setDescription(e.target.value)}
                            />
                        </Form.Item>
                    </>
                )}

                <Form.Item>
                    <Button
                        type="primary"
                        onClick={handleSubmission}
                        loading={loading}
                        disabled={loading}
                    >
                        Начислить
                    </Button>
                </Form.Item>
            </Form>
        </div>
    );
};

export default ChargePage;