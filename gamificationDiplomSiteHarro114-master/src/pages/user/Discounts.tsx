import React, {useEffect, useState} from 'react';
import {Card, Row, Col, Button, Spin} from 'antd';
import http from "../../services/authService";

interface Discount {
    id: number;
    name: string;
    description: string;
    discountSize: number;
    isActivated: boolean;
}

export const getDiscount = async (): Promise<Discount[]> => {
    const response = await http.get('/api/Profile/userDiscounts');
    return response.data.discounts || [];
};

const Discounts = () => {
    const [discounts, setDiscounts] = useState<Discount[]>([]);
    const [loading, setLoading] = useState(true);
    const [activatingId, setActivatingId] = useState<number | null>(null);

    useEffect(() => {
        const fetchDiscounts = async () => {
            setLoading(true);
            try {
                const result = await getDiscount();
                setDiscounts(result);
            } catch (err) {
                console.error("Ошибка загрузки скидок", err);
                setDiscounts([]);
            } finally {
                setLoading(false);
            }
        };
        fetchDiscounts();
    }, []);

    const handleActivate = async (id_d: number) => {
        setActivatingId(id_d);
        try {
            await http.post(`/api/discounts/ActivatedDiscount`, {
                id: id_d,
            });

            setDiscounts(prevDiscounts =>
                prevDiscounts.map(discount =>
                    discount.id === id_d
                        ? {...discount, isActivated: true}
                        : discount
                )
            );
        } catch (error) {
            console.error('Ошибка активации скидки', error);
        } finally {
            setActivatingId(null);
        }
    };


    return (
        <Row gutter={[16, 16]}>
            {discounts.map(discount => (
                <Col key={discount.id} xs={24} sm={12} md={8}>
                    <Card hoverable>
                        <Card.Meta
                            title={`${discount.name} — ${discount.discountSize}%`}
                            description={`${discount.description}`}
                        />
                        <div style={{marginTop: 16}}>
                            {discount.isActivated ? (
                                <span style={{color: 'green', fontWeight: 'bold'}}>
                                    Уже в магазине
                                </span>
                            ) : (
                                <Button
                                    type="primary"
                                    loading={activatingId === discount.id}
                                    onClick={() => handleActivate(discount.id)}
                                >
                                    Активировать
                                </Button>
                            )}
                        </div>
                    </Card>
                </Col>
            ))}
        </Row>
    );
};

export default Discounts;