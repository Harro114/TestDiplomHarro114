import React, {Fragment, useEffect, useState} from 'react';
import {Form, Input, Switch, InputNumber, DatePicker, Button, notification, Transfer} from 'antd';
import {CloseOutlined} from '@ant-design/icons';
import {useNavigate, useParams} from 'react-router-dom';
import moment from 'moment';
import http from '../../services/authService';


interface DataItem {
    id: number;
    name: string;
    isActive: boolean;
}

const DiscountForm: React.FC = () => {
    const [form] = Form.useForm();
    const [products, setProducts] = useState<DataItem[]>([]);
    const [categories, setCategories] = useState<DataItem[]>([]);
    const [selectedProducts, setSelectedProducts] = useState<number[]>([]);
    const [selectedCategories, setSelectedCategories] = useState<number[]>([]);
    const {id} = useParams();
    const navigate = useNavigate();
    const [api, contextHolder] = notification.useNotification();


    const fetchProducts = async () => {
        try {
            const response = await http.get('/api/Admin/getAllProducts');
            const activeProducts = response.data.filter((p: DataItem) => p.isActive);
            setProducts(activeProducts);
        } catch (error) {
            console.error('Ошибка загрузки продуктов:', error);
        }
    };

    const fetchCategories = async () => {
        try {
            const response = await http.get('/api/Admin/getAllCategories');
            const activeCategories = response.data.filter((c: DataItem) => c.isActive);
            setCategories(activeCategories);
        } catch (error) {
            console.error('Ошибка загрузки категорий:', error);
        }
    };


    const fetchDiscount = async () => {
        if (!id) return;
        try {
            const response = await http.get(`/api/admin/getDiscount?discountId=${id}`);
            const data = response.data;
            form.setFieldsValue({
                name: data.name,
                description: data.description,
                isActive: data.isActive,
                discountSize: data.discountSize,
                amount: data.amount,
                isPrimary: data.isPrimary,
                startDate: data.startDate ? moment(data.startDate) : null,
                endDate: data.endDate ? moment(data.endDate) : null,
            });

            setSelectedProducts(data.productsId?.map((p: DataItem) => p.id) || []);
            setSelectedCategories(data.categoriesId?.map((c: DataItem) => c.id) || []);
        } catch (error) {
            const err = error as { response?: { data?: { message?: string } } };
            api.error({
                message: 'Ошибка загрузки скидки',
                description: err.response?.data?.message || 'Ошибка сервера',
            });
        }
    };

    const handleSubmit = async (values: any) => {
        const payload = {
            ...values,
            startDate: values.startDate ? values.startDate.toISOString() : null,
            endDate: values.endDate ? values.endDate.toISOString() : null,
            productsId: selectedProducts.map((id) => {
                const product = products.find((p) => p.id === id);
                return product ? {id: product.id, name: product.name, isActive: product.isActive} : null;
            }).filter(Boolean),
            categoriesId: selectedCategories.map((id) => {
                const category = categories.find((c) => c.id === id);
                return category ? {id: category.id, name: category.name, isActive: category.isActive} : null;
            }).filter(Boolean),

            id: id,
        };

        try {
            if (id) {
                await http.post('/api/admin/updateDiscount', payload);
                notification.success({message: 'Скидка обновлена'});
            } else {
                await http.post('/api/Admin/createDiscount', payload);
                notification.success({message: 'Скидка создана'});
            }
            navigate('/admin/discounts');
        } catch (error: any) {
            api.error({
                message: 'Ошибка сохранения скидки',
                description: error.response?.data?.message || 'Ошибка сервера',
            });
        }
    };

    useEffect(() => {
        fetchProducts();
        fetchCategories();
        if (id) fetchDiscount();
    }, [id]);

    return (
        <div style={{position: 'relative'}}>
            <Button
                icon={<CloseOutlined/>}
                style={{position: 'absolute', right: 0, top: 0}}
                onClick={() => navigate('/admin/discounts')}
            />
            {contextHolder}
            <Form form={form} layout="vertical" onFinish={handleSubmit}>
                <Form.Item label="Название" name="name" rules={[{required: true, message: 'Введите название'}]}>
                    <Input/>
                </Form.Item>
                <Form.Item label="Описание" name="description" rules={[{required: true}]}>
                    <Input/>
                </Form.Item>
                <Form.Item label="Активна" name="isActive" valuePropName="checked">
                    <Switch/>
                </Form.Item>
                <Form.Item label="Размер скидки" name="discountSize" rules={[{required: true}]}>
                    <InputNumber min={0} max={100}/>
                </Form.Item>
                <Form.Item label="Стоимость" name="amount" rules={[{required: true}]}>
                    <InputNumber min={0}/>
                </Form.Item>
                <Form.Item label="Дата начала" name="startDate">
                    <DatePicker/>
                </Form.Item>
                <Form.Item label="Дата окончания" name="endDate">
                    <DatePicker/>
                </Form.Item>
                <Form.Item label="Первичная скидка" name="isPrimary" valuePropName="checked">
                    <Switch/>
                </Form.Item>
                <Form.Item label="Товары">
                    <Transfer
                        dataSource={products.map((product) => ({
                            key: product.id.toString(),
                            title: product.name,
                        }))}
                        targetKeys={selectedProducts.map((id) => id.toString())}
                        onChange={(targetKeys) => setSelectedProducts(targetKeys.map(Number))}
                        render={(item) => item.title}
                    />
                </Form.Item>
                <Form.Item label="Категории">
                    <Transfer
                        dataSource={categories.map((category) => ({
                            key: category.id.toString(),
                            title: category.name,
                        }))}
                        targetKeys={selectedCategories.map((id) => id.toString())}
                        onChange={(targetKeys) => setSelectedCategories(targetKeys.map(Number))}
                        render={(item) => item.title}
                    />
                </Form.Item>
                <Button type="primary" htmlType="submit" style={{marginRight: '10px'}}>
                    Сохранить
                </Button>
                <Button onClick={() => navigate('/admin/discounts')}>Отмена</Button>
            </Form>
        </div>
    );
};

export default DiscountForm;