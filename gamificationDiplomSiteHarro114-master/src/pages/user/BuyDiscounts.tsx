import React, {useEffect, useState} from 'react';
import {Row, Col, Card, Button, message, notification, List, Typography} from 'antd';
import http from "../../services/authService";

export interface PrimaryDiscount {
    id: number;
    name: string;
    description: string;
    discountSize: number;
    amount: number;
    isActive: boolean;
    startDate: string | null;
    endDate: string | null;
    productsStore: string[] | null;
    categoriesStore: string[] | null;
}

type NotificationType = 'success' | 'info' | 'warning' | 'error';


const BuyDiscounts: React.FC = () => {
    const [primaryDiscounts, setPrimaryDiscounts] = useState<PrimaryDiscount[]>([]);
    const [loading, setLoading] = useState(false);
    const [api, contextHolder] = notification.useNotification();

    const openNotificationWithIcon = (type: NotificationType, name: string, descript: string) => {
        api[type]({
            message: name,
            description: descript
        });
    };


    useEffect(() => {
        const fetchPrimaryDiscounts = async () => {
            setLoading(true);
            try {
                const response = await http.get('/api/discounts/getPrimaryDiscount');
                setPrimaryDiscounts(response.data);
            } catch (error) {
                message.error('Ошибка при загрузке первичных скидок.');
            } finally {
                setLoading(false);
            }
        };

        fetchPrimaryDiscounts();
    }, []);

    const handlePurchase = async (discountId: number) => {
        setLoading(true);
        try {
            const response = await http.post('/api/discounts/buyPrimaryDiscount', {
                DiscountId: discountId,
            });

            if (response.status === 201) {
                openNotificationWithIcon("success", 'Покупка оформлена', 'Во вкладке скидки уже можете найти покупку'
                );
            }
        } catch (error: any) {
            if (error.response?.status === 400) {
                openNotificationWithIcon(
                    "error",
                    'Недостаточно валюты',
                    'У вас недостаточно валюты, чтобы совершить покупку.'
                );
            } else {
                openNotificationWithIcon(
                    "error",
                    'Ошибка при покупке',
                    'Произошла непредвиденная ошибка. Попробуйте снова позже.'
                );
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            {contextHolder}

            <Row gutter={[16, 16]}>
                {primaryDiscounts.map((discount) => (
                    <Col span={8} key={discount.id}>
                        <Card
                            title={discount.name}
                            bordered
                            hoverable
                            extra={`Цена: ${discount.amount} валюты`}
                        >
                            <p>{discount.description}</p>
                            <p>Размер скидки: <b>{discount.discountSize}%</b></p>

                            {discount.productsStore && (
                                <>
                                    <Typography.Text strong>Товары:</Typography.Text>
                                    <List
                                        size="small"
                                        dataSource={discount.productsStore}
                                        renderItem={(product) => <List.Item>- {product}</List.Item>}
                                    />
                                </>
                            )}

                            {discount.categoriesStore && (
                                <>
                                    <Typography.Text strong>Категории:</Typography.Text>
                                    <List
                                        size="small"
                                        dataSource={discount.categoriesStore}
                                        renderItem={(category) => <List.Item>- {category}</List.Item>}
                                    />
                                </>
                            )}

                            <div style={{marginTop: 20, textAlign: 'center'}}>
                                <Button
                                    type="primary"
                                    onClick={() => handlePurchase(discount.id)}
                                    disabled={!discount.isActive || loading}
                                >
                                    Купить
                                </Button>
                            </div>
                        </Card>
                    </Col>
                ))}
            </Row>
        </>
    );
};

export default BuyDiscounts;