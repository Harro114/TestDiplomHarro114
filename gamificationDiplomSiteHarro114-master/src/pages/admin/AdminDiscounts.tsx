import React, {useEffect, useState} from 'react';
import {Table, Button, Modal, Switch, notification, Spin} from 'antd';
import {useNavigate} from 'react-router-dom';
import type {ColumnsType} from 'antd/es/table';
import http from "../../services/authService";

interface Discount {
    id: number;
    name: string;
    description: string;
    isActive: boolean;
    discountSize: number;
    startDate: string;
    endDate: string | null;
    amount: number;
    isPrimary: boolean;
}

const AdminDiscounts: React.FC = () => {
    const [discounts, setDiscounts] = useState<Discount[]>([]);
    const [loading, setLoading] = useState(false);
    const [api, contextHolder] = notification.useNotification();
    const navigate = useNavigate();
    const [isDeleteModalVisible, setDeleteModalVisible] = useState(false);
    const [selectedDiscountId, setSelectedDiscountId] = useState<number | null>(null);

    const fetchDiscounts = async () => {
        setLoading(true);
        try {
            const response = await http.get('/api/Admin/getAllDiscounts');
            setDiscounts(response.data);
        } catch (error: any) {
            if (error.response?.status === 400) {
                api.error({
                    message: 'Ошибка загрузки скидок',
                    description: error.response.data.message || 'Ошибка сервера',
                });
            }
        } finally {
            setLoading(false);
        }
    };

    const handleSwitchActivity = async (id: number, isActive: boolean) => {
        try {
            await http.post('/api/Admin/SwitchActivityDiscount', {
                DiscountId: id,
                IsActive: isActive,
            });
            notification.success({
                message: 'Активность изменена',
                description: `Скидка ${isActive ? 'активирована' : 'деактивирована'}.`,
            });
            fetchDiscounts();
        } catch (error: any) {
            notification.error({
                message: 'Ошибка изменения активности',
                description: error.response?.data?.message || 'Ошибка сервера',
            });
        }
    };

    const showDeleteModal = (id: number) => {
        setSelectedDiscountId(id);
        setDeleteModalVisible(true);
    };

    const handleDeleteDiscount = async () => {
        if (!selectedDiscountId) return;

        try {
            setLoading(true);
            const response = await http.post('/api/Admin/deleteDiscount', {
                DiscountId: selectedDiscountId,
            });

            if (response.status === 200) {
                notification.success({
                    message: 'Скидка удалена',
                    description: `Скидка с ID ${selectedDiscountId} успешно удалена.`,
                });
                fetchDiscounts();
            }
        } catch (error: any) {
            notification.error({
                message: 'Ошибка удаления',
                description: error.response?.data?.message || 'Произошла ошибка при удалении скидки.',
            });
        } finally {
            setLoading(false);
            setDeleteModalVisible(false);
            setSelectedDiscountId(null);
        }
    };

    const handleCancelDelete = () => {
        setDeleteModalVisible(false);
        setSelectedDiscountId(null);
    };


    const handleCreateDiscount = () => {
        navigate('/admin/create-discount');
    };

    const handleEditDiscount = (id: number) => {
        navigate(`/admin/edit-discount/${id}`);
    };

    const columns: ColumnsType<Discount> = [
        {
            title: 'ID',
            dataIndex: 'id',
            key: 'id',
        },
        {
            title: 'Название',
            dataIndex: 'name',
            key: 'name',
        },
        {
            title: 'Описание',
            dataIndex: 'description',
            key: 'description',
        },
        {
            title: 'Размер (%)',
            dataIndex: 'discountSize',
            key: 'discountSize',
            render: (text) => `${text}%`,
        },
        {
            title: 'Стоимость',
            dataIndex: 'amount',
            key: 'amount',
        },
        {
            title: 'Активность',
            dataIndex: 'isActive',
            key: 'isActive',
            render: (isActive, record) => (
                <Switch
                    checked={isActive}
                    onChange={(checked) => handleSwitchActivity(record.id, checked)}
                />
            ),
        },
        {
            title: 'Действия',
            key: 'actions',
            render: (_, record) => (
                <>
                    <Button
                        type="link"
                        onClick={() => handleEditDiscount(record.id)}
                        style={{marginRight: 8}}
                    >
                        Редактировать
                    </Button>
                    <Button
                        type="primary"
                        danger
                        onClick={() => showDeleteModal(record.id)}
                    >
                        Удалить
                    </Button>
                </>
            ),
        },
    ];

    useEffect(() => {
        fetchDiscounts();
    }, []);

    return (
        <Spin spinning={loading}>
            <div>
                {contextHolder}

                <Button
                    type="primary"
                    onClick={handleCreateDiscount}
                    style={{marginBottom: 16}}
                >
                    Создать новую скидку
                </Button>

                <Table
                    columns={columns}
                    dataSource={discounts}
                    rowKey="id"
                    pagination={{pageSize: 10}}
                />
            </div>

            <Modal
                title="Удалить скидку"
                visible={isDeleteModalVisible}
                onOk={handleDeleteDiscount}
                onCancel={handleCancelDelete}
                okText="Удалить"
                cancelText="Отмена"
                confirmLoading={loading}
            >
                <p>Вы уверены, что хотите удалить эту скидку? Это действие необратимо.</p>
            </Modal>
        </Spin>
    );
};

export default AdminDiscounts;