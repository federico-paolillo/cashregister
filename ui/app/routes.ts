import { type RouteConfig, index, route } from "@react-router/dev/routes";

export default [
  index("routes/order/order.tsx"),
  route("articles", "routes/articles/articles.tsx"),
  route("articles/bulk", "routes/articles-bulk/articles-bulk.tsx"),
  route("order/:orderId", "routes/order-view/order-view.tsx"),
  route("orders", "routes/order-overview/order-overview.tsx"),
] satisfies RouteConfig;
