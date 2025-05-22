import React, {useEffect, useState} from "react";
import {Layout, Menu} from "antd";
import {Link, Outlet} from "react-router-dom";
import http from "../services/authService";

const {Header, Content} = Layout;

export const UserLayout: React.FC = () => {
    const [roleId, setRoleId] = useState<number | null>(null);
    const [expCount, setExpCount] = useState<number | null>(null);

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const roleResponse = await http.get("/api/profile/checkRole");
                setRoleId(roleResponse.data.roleId);

                const expResponse = await http.get("/api/profile/getExpCount");
                setExpCount(expResponse.data);
            } catch (error) {
                console.error("Ошибка при загрузке данных пользователя", error);
            }
        };

        fetchUserData();
    }, []);

    return (
        <Layout>
            <Header>
                <Menu theme="dark" mode="horizontal">
                    <Menu.Item key="profile">
                        <Link to="/user/profile">Профиль</Link>
                    </Menu.Item>
                    <Menu.Item key="discounts">
                        <Link to="/user/discounts">Скидки</Link>
                    </Menu.Item>
                    <Menu.Item key="exchange">
                        <Link to="/user/exchange">Обмен</Link>
                    </Menu.Item>
                    <Menu.Item key="BuyDiscounts">
                        <Link to="/user/buydiscounts">Покупка скидок</Link>
                    </Menu.Item>
                    {roleId === 2 && (
                        <Menu.Item key="admin">
                            <Link to="/admin">Админка</Link>
                        </Menu.Item>
                    )}
                    {expCount !== null && (
                        <Menu.Item key="exp" disabled style={{color: "white"}}>
                            {`${expCount} Exp`}
                        </Menu.Item>
                    )}
                </Menu>
            </Header>
            <Content style={{padding: "20px"}}>
                <Outlet/>
            </Content>
        </Layout>
    );
};