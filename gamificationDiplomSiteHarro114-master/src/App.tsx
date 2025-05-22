import {useRoutes, Navigate} from "react-router-dom";
import {UserLayout} from "./layouts/UserLayout";
import Profile from "./pages/user/Profile";
import Discounts from "./pages/user/Discounts";
import Exchange from "./pages/user/Exchange";
import {AdminLayout} from "./layouts/AdminLayout";
import Users from "./pages/admin/Users";
import Login from "./pages/Login";
import BuyDiscounts from "./pages/user/BuyDiscounts";
import Cookies from "js-cookie";
import {DndProvider} from 'react-dnd';
import {HTML5Backend} from 'react-dnd-html5-backend';
import 'antd/dist/reset.css'
import AdminDiscounts from "./pages/admin/AdminDiscounts";
import DiscountForm from "./pages/admin/DiscountForm";
import ChargePage from "./pages/admin/ChargePage";
import DiscountsManagement from "./pages/admin/DiscountsManagement";
import NotFoundPage from "./pages/user/NotFoundPage";
import DiscountsExchangeManagement from "./pages/admin/DiscountsExchangeManagement";
import CreateOrEditDiscountExchange from "./pages/admin/CreateOrEditDiscountExchange";
import Blocked from "./pages/Blocked";

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({children}) => {
    const token = Cookies.get("authToken");
    if (!token) return <Navigate to="/login" replace/>;
    return <>{children}</>;
};

const App: React.FC = () => {
    const routes = useRoutes([
        {
            path: "/user",
            element: (
                <ProtectedRoute>
                    <UserLayout/>
                </ProtectedRoute>
            ),
            children: [
                {
                    path: "profile",
                    element: <Profile/>
                },
                {
                    path: "discounts",
                    element: <Discounts/>
                },
                {
                    path: "exchange",
                    element: <Exchange/>
                },
                {
                    path: "buydiscounts",
                    element: <BuyDiscounts/>
                },
            ],
        },
        {
            path: "/admin",
            element: (
                <ProtectedRoute>
                    <AdminLayout/>
                </ProtectedRoute>
            ),
            children: [
                {
                    path: "users",
                    element: <Users/>
                },
                {
                    path: 'discounts',
                    element: <AdminDiscounts/>
                },
                {
                    path: 'create-discount',
                    element: <DiscountForm/>
                },
                {
                    path: 'edit-discount/:id',
                    element: <DiscountForm/>
                },
                {
                    path: "chargePage",
                    element: <ChargePage/>
                },
                {
                    path: "discountsManagement",
                    element: <DiscountsManagement/>
                },
                {
                    path: "discounts-exchange-management",
                    element: <DiscountsExchangeManagement/>
                },
                {
                    path: "create-discount-exchange",
                    element: <CreateOrEditDiscountExchange/>
                },
                {
                    path: 'edit-discount-exchange/:id',
                    element: <CreateOrEditDiscountExchange/>
                }
            ],
        },
        {
            path: "/login",
            element: <Login/>
        },
        {
            path: "*",
            element: <NotFoundPage/>
        },
        {
            path: "/",
            element: <Profile/>
        },
        {
            path: "/blocked",
            element: <Blocked/>
        }
    ]);

    return <DndProvider backend={HTML5Backend}>{routes}</DndProvider>;
};

export default App;
