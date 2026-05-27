export interface ArticlesPageDto {
  next: string | null;
  hasNext: boolean;
  items: ArticleListItemDto[];
}

export interface ArticleListItemDto {
  id: string;
  description: string;
  priceInCents: number;
  printDetailReceipt: boolean;
  quantityAvailable: number | null;
}

export interface ArticleDto {
  id: string;
  description: string;
  priceInCents: number;
  printDetailReceipt: boolean;
  quantityAvailable: number | null;
}

export interface RegisterArticleRequestDto {
  description: string;
  priceInCents: number;
  printDetailReceipt: boolean;
  quantityAvailable: number | null;
}

export interface ChangeArticleRequestDto {
  description: string;
  priceInCents: number;
  printDetailReceipt: boolean;
  quantityAvailable: number | null;
}

export interface OrdersPageDto {
  next: string | null;
  hasNext: boolean;
  items: OrderListItemDto[];
}

export interface OrderListItemDto {
  id: string;
  number: string;
  totalInCents: number;
  totalOverrideInCents: number | null;
  date: number;
}

export interface PlaceOrderRequestDto {
  items: PlaceOrderItemDto[];
  totalOverrideInCents?: number;
}

export interface PlaceOrderItemDto {
  article: string;
  quantity: number;
}

export interface OrderItemDto {
  id: string;
  article: string;
  description: string;
  priceInCents: number;
  quantity: number;
}

export interface OrderDto {
  id: string;
  number: string;
  date: number;
  totalInCents: number;
  totalOverrideInCents: number | null;
  items: OrderItemDto[];
}

export interface EntityPointerDto {
  id: string;
  location: string;
}

export interface DeviceDto {
  id: string;
  name: string;
  target: string;
  description: string | null;
  selected: boolean;
}

export interface StatisticsDto {
  articles: ArticleInventoryItemDto[];
  orders: OrderStatisticsItemDto[];
  summary: OrderStatisticsSummaryDto;
}

export interface ArticleInventoryItemDto {
  articleId: string;
  description: string;
  retired: boolean;
  soldUnits: number;
}

export interface OrderStatisticsItemDto {
  orderId: string;
  orderNumber: string;
  date: number;
  producedArticles: number;
  expectedVolumeInCents: number;
  realVolumeInCents: number;
  deltaInCents: number;
  hasOverride: boolean;
}

export interface OrderStatisticsSummaryDto {
  orderCount: number;
  producedArticles: number;
  expectedVolumeInCents: number;
  realVolumeInCents: number;
  deltaInCents: number;
}
