import React, {useEffect, useState} from 'react';
import {Form, Input, Select, Button} from 'antd';
import {useParams, useNavigate} from 'react-router-dom';
import http from "../../services/authService";

const {Option} = Select;

interface Discount {
    id: number;
    name: string;
}

const CreateOrEditDiscountExchange: React.FC = () => {
    const [discounts, setDiscounts] = useState<Discount[]>([]);
    const [primaryDiscounts, setPrimaryDiscounts] = useState<Discount[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [form] = Form.useForm();
    const {id} = useParams<{ id?: string }>();
    const navigate = useNavigate();

    const fetchDiscountData = async () => {
        try {
            const [discountResponse, primaryResponse] = await Promise.all([
                http.get('/api/Admin/getAllDiscounts'),
                http.get('/api/Admin/getDiscountsNoPrimary'),
            ]);

            setDiscounts(discountResponse.data);
            setPrimaryDiscounts(primaryResponse.data);
        } catch (error) {
            console.error('Ошибка при загрузке данных скидок:', error);
        }
    };

    const fetchExchangeData = async (id: string) => {
        try {
            const response = await http.get(`/api/Admin/getExchangeDiscount?id=${id}`);
            const data = response.data[0];

            form.setFieldsValue({
                discountId: data.discountId,
                discountExchangeOneId: data.discountExchangeOneId,
                discountExchangeTwoId: data.discountExchangeTwoId,
            });
        } catch (error) {
            console.error('Ошибка при загрузке данных для редактирования:', error);
        }
    };

    useEffect(() => {
        fetchDiscountData();
        if (id) {
            fetchExchangeData(id);
        }
    }, [id]);

    const onFinish = async (values: any) => {
        setLoading(true);

        try {
            if (id) {
                await http.post('/api/Admin/updateDiscountExchange', {
                    Id: Number(id),
                    ...values,
                });
            } else {
                await http.post('/api/Admin/createDiscountExchange', values);
            }

            navigate('/admin/discounts-exchange-management');
        } catch (error) {
            console.error('Ошибка при сохранении:', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Form form={form} onFinish={onFinish} layout="vertical">
            <Form.Item
                name="discountId"
                label="Основная скидка"
                rules={[{required: true, message: 'Пожалуйста, выберите основную скидку'}]}
            >
                <Select placeholder="Выберите основную скидку">
                    {primaryDiscounts.map((discount) => (
                        <Option key={discount.id} value={discount.id}>
                            {discount.name}
                        </Option>
                    ))}
                </Select>
            </Form.Item>

            <Form.Item
                name="discountExchangeOneId"
                label="1-я скидка для объединения"
                rules={[{required: true, message: 'Пожалуйста, выберите 1-ю скидку для объединения'}]}
            >
                <Select placeholder="Выберите 1-ю скидку для объединения">
                    {discounts.map((discount) => (
                        <Option key={discount.id} value={discount.id}>
                            {discount.name}
                        </Option>
                    ))}
                </Select>
            </Form.Item>

            <Form.Item
                name="discountExchangeTwoId"
                label="2-я скидка для объединения"
                rules={[{required: true, message: 'Пожалуйста, выберите 2-ю скидку для объединения'}]}
            >
                <Select placeholder="Выберите 2-ю скидку для объединения">
                    {discounts.map((discount) => (
                        <Option key={discount.id} value={discount.id}>
                            {discount.name}
                        </Option>
                    ))}
                </Select>
            </Form.Item>

            <Button type="primary" htmlType="submit" loading={loading}>
                {id ? 'Сохранить изменения' : 'Создать'}
            </Button>
        </Form>
    );
};

export default CreateOrEditDiscountExchange;