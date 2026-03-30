export interface ArticlesPageDto {
  next: string | null;
  hasNext: boolean;
  items: ArticleListItemDto[];
}

export interface ArticleListItemDto {
  id: string;
  description: string;
  price: number;
}

export interface RegisterArticleRequestDto {
  description: string;
  priceInCents: number;
}

export interface ChangeArticleRequestDto {
  description: string;
  priceInCents: number;
}

export interface OrdersPageDto {
  next: string | null;
  hasNext: boolean;
  items: OrderListItemDto[];
}

export interface OrderListItemDto {
  id: string;
  number: string;
  total: number;
  date: number;
}

export interface PlaceOrderRequestDto {
  items: PlaceOrderItemDto[];
  totalOverride?: number;
}

export interface PlaceOrderItemDto {
  article: string;
  quantity: number;
}

export interface OrderItemDto {
  id: string;
  article: string;
  description: string;
  price: number;
  quantity: number;
}

export interface OrderDto {
  id: string;
  number: string;
  date: number;
  total: number;
  totalOverride: number | null;
  items: OrderItemDto[];
}
