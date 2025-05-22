import React, {useRef} from 'react';
import {Card} from 'antd';
import {useDrag, useDrop} from 'react-dnd';
import {Discount} from './Exchange';

interface DiscountCardProps {
    discount: Discount;
    onDragStart: (discount: Discount) => void;
    onDrop: () => void;
}

const DiscountCard: React.FC<DiscountCardProps> = ({discount, onDragStart, onDrop}) => {
    const ref = useRef<HTMLDivElement>(null);

    const [, dragRef] = useDrag({
        type: 'discount',
        item: () => {
            console.log('Выбранная карточка:', discount);
            onDragStart(discount);
            return {...discount};
        },
    });

    const [, dropRef] = useDrop({
        accept: 'discount',
        drop: () => {
            onDrop();
        },
    });

    dragRef(dropRef(ref));

    return (
        <div ref={ref}>
            <Card title={discount.name} bordered>
                <p>Категория: {discount.description}</p>
                <p>Скидка: {discount.discountSize}%</p>
            </Card>
        </div>
    );
};

export default DiscountCard;