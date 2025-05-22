import React from "react";
import {Layout, Menu} from "antd";
import {Link, Outlet} from "react-router-dom";

const {Header, Content, Footer} = Layout;

export const AdminLayout: React.FC = () => (
    <Layout>
        <Header>
            <Menu theme="dark" mode="horizontal">
                <Menu.Item key="users">
                    <Link to="/admin/users">Пользователи</Link>
                </Menu.Item>
                <Menu.Item key="discounts">
                    <Link to="/admin/discounts">Скидки</Link>
                </Menu.Item>
                <Menu.Item key="ChargePage">
                    <Link to="/admin/chargePage">Начисление</Link>
                </Menu.Item>
                <Menu.Item key="discountsManagement">
                    <Link to="/admin/discountsManagement">Истории таблиц</Link>
                </Menu.Item>
                <Menu.Item key="discounts-exchange-management">
                    <Link to="/admin/discounts-exchange-management">Объединения</Link>
                </Menu.Item>
                <Menu.Item key="userslayout">
                    <Link to="/user/profile">Пользовательская часть</Link>
                </Menu.Item>
            </Menu>
        </Header>
        <Content style={{padding: "20px"}}>
            <Outlet/>
        </Content>
    </Layout>
);