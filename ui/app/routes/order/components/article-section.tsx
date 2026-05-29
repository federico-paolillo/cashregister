import { useId } from "react";
import type { ArticleListItemDto } from "@cashregister/model";
import { ArticleButton } from "./article-button";

interface ArticleSectionProps {
  title: string;
  articles: ArticleListItemDto[];
  lowQuantityArticleIds: Set<string>;
  onSelect: (article: ArticleListItemDto) => void;
}

export function ArticleSection({
  title,
  articles,
  lowQuantityArticleIds,
  onSelect,
}: ArticleSectionProps) {
  const titleId = useId();

  return (
    <section aria-labelledby={titleId}>
      <div className="mb-3 flex items-center gap-3">
        <h2 id={titleId} className="text-sm font-semibold text-gray-700">
          {title}
        </h2>
        <div className="h-px flex-1 bg-gray-200" />
      </div>
      <div className="grid grid-cols-3 gap-3">
        {articles.map((article) => (
          <ArticleButton
            key={article.id}
            article={article}
            lowQuantity={lowQuantityArticleIds.has(article.id)}
            onSelect={onSelect}
          />
        ))}
      </div>
    </section>
  );
}
