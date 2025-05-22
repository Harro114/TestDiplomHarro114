import React, {useEffect, useState} from 'react';
import {Table, Tabs, notification} from 'antd';
import http from '../../services/authService';

const {TabPane} = Tabs;

const DiscountsManagement: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const [usersDiscounts, setUsersDiscounts] = useState([]);
    const [usersDiscountsHistory, setUsersDiscountsHistory] = useState([]);
    const [activatedDiscounts, setActivatedDiscounts] = useState([]);
    const [activatedDiscountsHistory, setActivatedDiscountsHistory] = useState([]);
    const [expChanges, setExpChanges] = useState([]);

    const [api, contextHolder] = notification.useNotification();

    const fetchData = async () => {
        setLoading(true);
        try {
            const [usersDiscountsRes, usersDiscountsHistoryRes, activatedDiscountsRes, activatedDiscountsHistoryRes, expChangesRes] = await Promise.all([
                http.get('/api/Admin/GetUsersDiscounts'),
                http.get('/api/Admin/getUserDiscountsHistory'),
                http.get('/api/Admin/getUserDiscountsActivated'),
                http.get('/api/Admin/getUserDiscountsActivatedHistory'),
                http.get('/api/Admin/GetExpChanges'),
            ]);

            setUsersDiscounts(usersDiscountsRes.data);
            setUsersDiscountsHistory(usersDiscountsHistoryRes.data);
            setActivatedDiscounts(activatedDiscountsRes.data);
            setActivatedDiscountsHistory(activatedDiscountsHistoryRes.data);
            setExpChanges(expChangesRes.data);
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка получения данных',
                    description: error.response.data?.message || 'Ошибка сервера',
                });
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    const columns = {
        usersDiscounts: [
            {
                title: 'Имя пользователя',
                dataIndex: 'username',
                key: 'username',
            },
            {
                title: 'Скидка',
                dataIndex: 'discountName',
                key: 'discountName',
            },
            {
                title: 'Дата начисления',
                dataIndex: 'dateAccruals',
                key: 'dateAccruals',
            },
        ],
        usersDiscountsHistory: [
            {title: 'Имя пользователя', dataIndex: 'username', key: 'username'},
            {title: 'Скидка', dataIndex: 'discountName', key: 'discountName'},
            {title: 'Дата начисления', dataIndex: 'dateAccruals', key: 'dateAccruals'},
            {title: 'Дата удаления', dataIndex: 'dateDelete', key: 'dateDelete'},
        ],
        activatedDiscounts: [
            {title: 'Имя пользователя', dataIndex: 'username', key: 'username'},
            {title: 'Скидка', dataIndex: 'discountName', key: 'discountName'},
            {title: 'Дата активации', dataIndex: 'dateActivateDiscount', key: 'dateActivateDiscount'},
        ],
        activatedDiscountsHistory: [
            {title: 'Имя пользователя', dataIndex: 'username', key: 'username'},
            {title: 'Скидка', dataIndex: 'discountName', key: 'discountName'},
            {title: 'Дата активации', dataIndex: 'dateActivateDiscount', key: 'dateActivateDiscount'},
            {title: 'Дата удаления', dataIndex: 'dateDelete', key: 'dateDelete'},
        ],
        expChanges: [
            {title: 'Имя пользователя', dataIndex: 'username', key: 'username'},
            {title: 'Сумма', dataIndex: 'value', key: 'value'},
            {title: 'Текущий баланс', dataIndex: 'currentBalance', key: 'currentBalance'},
            {title: 'Дата изменения', dataIndex: 'createdAt', key: 'createdAt'},
            {title: 'Описание', dataIndex: 'discription', key: 'discription'},
        ],
    };


    return (
        <div style={{padding: '20px'}}>
            {contextHolder}
            <h2>Управление скидками и балансом</h2>
            <Tabs defaultActiveKey="1">
                <TabPane tab="Текущие скидки" key="1">
                    <Table
                        columns={columns.usersDiscounts}
                        dataSource={usersDiscounts}
                        loading={loading}
                        rowKey="id"
                    />
                </TabPane>
                <TabPane tab="История удаленных скидок" key="2">
                    <Table
                        columns={columns.usersDiscountsHistory}
                        dataSource={usersDiscountsHistory}
                        loading={loading}
                        rowKey="id"
                    />
                </TabPane>
                <TabPane tab="Активированные скидки" key="3">
                    <Table
                        columns={columns.activatedDiscounts}
                        dataSource={activatedDiscounts}
                        loading={loading}
                        rowKey="id"
                    />
                </TabPane>
                <TabPane tab="История активированных скидок" key="4">
                    <Table
                        columns={columns.activatedDiscountsHistory}
                        dataSource={activatedDiscountsHistory}
                        loading={loading}
                        rowKey="id"
                    />
                </TabPane>
                <TabPane tab="Изменения баланса" key="5">
                    <Table
                        columns={columns.expChanges}
                        dataSource={expChanges}
                        loading={loading}
                        rowKey="id"
                    />
                </TabPane>
            </Tabs>
        </div>
    );
};

export default DiscountsManagement;