import React, {useState, useEffect, useCallback} from 'react';
import {Row, Col, Modal, Button, message, notification, Space} from 'antd';
import {useDrop} from 'react-dnd';
import 'antd/dist/reset.css'

import update from 'immutability-helper';
import http from "../../services/authService";
import DiscountCard from './DiscountCard';

export interface Discount {
    id: number;
    discountId: number;
    name: string;
    description: string;
    discountSize: number;
}

type NotificationType = 'success' | 'info' | 'warning' | 'error';

interface DiscountCheckResponse {
    hasDiscount: boolean;
    discount: {
        id: number;
        name: string;
        description: string;
        amount: number;
        discountSize: number;
    };
}


const Exchange: React.FC = () => {
    const [discounts, setDiscounts] = useState<Discount[]>([]);
    const [draggedDiscount, setDraggedDiscount] = useState<Discount | null>(null);
    const [mergeTarget, setMergeTarget] = useState<Discount | null>(null);
    const [exchangePreview, setExchangePreview] = useState<DiscountCheckResponse['discount'] | null>(null);
    const [isModalVisible, setIsModalVisible] = useState(false);

    useEffect(() => {
        const fetchDiscounts = async () => {
            try {
                const response = await http.get('/api/discounts/getAllDiscountsUser');
                const fetchedDiscounts = response.data.discount.map((item: any) => ({
                    id: item.id,
                    discountId: item.discountId,
                    name: item.name,
                    description: item.description,
                    discountSize: item.discountSize,
                }));

                const ids = fetchedDiscounts.map((d: Discount) => d.id);
                const uniqueIds = new Set(ids);
                if (ids.length !== uniqueIds.size) {
                    console.error('Есть дублирующиеся id в массиве discounts:', ids);
                }

                setDiscounts(fetchedDiscounts);
            } catch (error) {
                message.error('Ошибка при загрузке скидок');
            }
        };

        fetchDiscounts();
    }, []);

    const openNotification = (type: NotificationType, notText: string) => {
        notification[type]({
            message: notText,
            description:
                '',
        });
    };

    const handleDrop = useCallback(
        async (target: Discount) => {
            if (!draggedDiscount || draggedDiscount.id === target.id) return;

            try {
                const response = await http.get<DiscountCheckResponse>(
                    `/api/discounts/checkExchange/${draggedDiscount.discountId}/${target.discountId}`
                );

                if (response.data.hasDiscount) {
                    setMergeTarget(target);
                    setExchangePreview(response.data.discount);
                    setIsModalVisible(true);
                } else {
                    message.error('Такой комбинации не существует');
                }
            } catch (err) {
                message.error('Ошибка при проверке объединения');
            }
        },
        [draggedDiscount]
    );

    const handleConfirmMerge = async () => {
        if (!draggedDiscount || !mergeTarget || !exchangePreview) return;

        try {
            const response = await http.post(
                '/api/discounts/CombiningDiscounts',
                {
                    DiscountOneId: draggedDiscount.discountId,
                    DiscountTwoId: mergeTarget.discountId,
                }
            );

            console.log("Response data:", response);


            if (response.status === 201) {
                console.log("Condition met for success notification");

                openNotification("success", "Скидки успешно объединены!");
            } else {
                console.warn("Unexpected status:", response.status);
            }

            const newDiscount = response.data.data.newDiscountUser.discounts;

            setDiscounts(prev => update(prev, {$push: [newDiscount]}));


            console.log("Discounts updated!", discounts);

        } catch (err) {
            console.error("Error while merging:", err);


            message.error('Ошибка при объединении');
        } finally {

            setIsModalVisible(false);
            setExchangePreview(null);
            setDraggedDiscount(null);
            setMergeTarget(null);
        }
    };

    return (
        <div>
            <Row gutter={[16, 16]}>
                {discounts.map((discount) => (
                    <Col key={discount.id} xs={24} sm={12} md={8}>
                        <DiscountCard
                            discount={discount}
                            onDragStart={() => setDraggedDiscount(discount)}
                            onDrop={() => handleDrop(discount)}
                        />
                    </Col>
                ))}
            </Row>

            <Modal
                title="Подтвердите объединение скидок"
                open={isModalVisible}
                onOk={handleConfirmMerge}
                onCancel={() => setIsModalVisible(false)}
                footer={[
                    <Button key="back" onClick={() => setIsModalVisible(false)}>
                        Отмена
                    </Button>,
                    <Button key="submit" type="primary" onClick={handleConfirmMerge}>
                        Объединить
                    </Button>,
                ]}
            >

                {exchangePreview && (
                    <div>
                        <p><b>Новая скидка:</b> {exchangePreview.name}</p>
                        <p><b>Описание:</b> {exchangePreview.description}</p>
                        <p><b>Размер скидки:</b> {exchangePreview.discountSize} %</p>
                        <p><b>Цена объединения:</b> {exchangePreview.amount}</p>
                    </div>
                )}
            </Modal>

        </div>
    );
};

export default Exchange;