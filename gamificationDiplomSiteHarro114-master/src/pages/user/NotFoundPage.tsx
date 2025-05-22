import React from 'react';
import {Button, Result} from 'antd';
import {useNavigate} from 'react-router-dom';

const NotFoundPage: React.FC = () => {
    const navigate = useNavigate();

    return (
        <Result
            status="404"
            title="404"
            subTitle="К сожалению, запрашиваемая страница не найдена."
            extra={
                <Button type="primary" onClick={() => navigate('/user/profile')}>
                    На главную
                </Button>
            }
        />
    );
};

export default NotFoundPage;