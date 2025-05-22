import React from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

const UnauthorizedPage: React.FC = () => {
    const navigate = useNavigate();

    return (
        <Result
            status="403"
            title="401"
            subTitle="У вас недостаточно прав для доступа к этой странице."
            extra={
                <Button type="primary" onClick={() => navigate('/login')}>
                    Войти
                </Button>
            }
        />
    );
};

export default UnauthorizedPage;