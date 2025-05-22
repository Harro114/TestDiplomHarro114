import React, { useEffect, useState } from 'react';
import { Table, Button, Space } from 'antd';
import { useNavigate } from 'react-router-dom';
import http from "../../services/authService";

interface ExchangeDiscount {
    id: number;
    name: string;
    discountExchangeOneName: string;
    discountExchangeTwoName: string;
}

const DiscountsExchangeManagement: React.FC = () => {
    const [exchangeDiscounts, setExchangeDiscounts] = useState<ExchangeDiscount[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const navigate = useNavigate();

    const fetchExchangeDiscounts = async () => {
        try {
            const response = await http.get('/api/Admin/getExchangeDiscounts');
            setExchangeDiscounts(response.data);
        } catch (error) {
            console.error('Ошибка при получении объединений скидок:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchExchangeDiscounts();
    }, []);

    const columns = [
        {
            title: 'Название скидки',
            dataIndex: 'name',
            key: 'name',
        },
        {
            title: 'Название 1-й скидки',
            dataIndex: 'discountExchangeOneName',
            key: 'discountExchangeOneName',
        },
        {
            title: 'Название 2-й скидки',
            dataIndex: 'discountExchangeTwoName',
            key: 'discountExchangeTwoName',
        },
        {
            title: 'Действия',
            key: 'actions',
            render: (record: ExchangeDiscount) => (
                <Button onClick={() => navigate(`/admin/edit-discount-exchange/${record.id}`)}>Редактировать</Button>
            ),
        },
    ];

    return (
        <div>
            <Space style={{ marginBottom: 16 }}>
                <Button type="primary" onClick={() => navigate('/admin/create-discount-exchange')}>
                    Создать новое объединение
                </Button>
            </Space>
            <Table
                columns={columns}
                dataSource={exchangeDiscounts}
                rowKey="id"
                loading={loading}
                pagination={{ pageSize: 10 }}
            />
        </div>
    );
};

export default DiscountsExchangeManagement;