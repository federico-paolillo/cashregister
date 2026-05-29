import type { ArticleListItemDto } from "@cashregister/model";
import { groupArticlesByInitial } from "./article-groups";
import { ArticleSection } from "./article-section";

interface ArticleSelectorProps {
  articles: ArticleListItemDto[];
  lowQuantityArticleIds: Set<string>;
  onSelect: (article: ArticleListItemDto) => void;
}

export function ArticleSelector({ articles, lowQuantityArticleIds, onSelect }: ArticleSelectorProps) {
  const articleGroups = groupArticlesByInitial(articles);

  return (
    <div className="space-y-6">
      {articleGroups.map((group) => (
        <ArticleSection
          key={group.title}
          title={group.title}
          articles={group.articles}
          lowQuantityArticleIds={lowQuantityArticleIds}
          onSelect={onSelect}
        />
      ))}
      {articles.length === 0 && (
        <p className="text-sm italic text-gray-500">
          No articles available.
        </p>
      )}
    </div>
  );
}
