import { type RouteConfig, index, route } from "@react-router/dev/routes";

export default [
  index("routes/home/home.tsx"),
  route("articles", "routes/articles/articles.tsx"),
  route("articles/bulk", "routes/articles-bulk/articles-bulk.tsx"),
  route("order", "routes/order/order.tsx"),
] satisfies RouteConfig;
