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
