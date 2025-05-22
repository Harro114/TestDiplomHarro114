import React from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

const UnauthorizedPage: React.FC = () => {
    const navigate = useNavigate();

    return (
        <Result
            status="error"
            title="401"
            subTitle="Вы заблокированы"
            extra={
                <Button type="primary" onClick={() => navigate('/login')}>
                    Войти
                </Button>
            }
        />
    );
};

export default UnauthorizedPage;