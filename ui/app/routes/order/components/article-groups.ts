import type { ArticleListItemDto } from "@cashregister/model";

export interface ArticleGroup {
  title: string;
  articles: ArticleListItemDto[];
}

const articleDescriptionCollator = new Intl.Collator(undefined, {
  sensitivity: "base",
});

export function groupArticlesByInitial(articles: ArticleListItemDto[]): ArticleGroup[] {
  const groups = new Map<string, ArticleListItemDto[]>();

  for (const article of articles) {
    const groupTitle = getArticleGroupTitle(article.description);
    groups.set(groupTitle, [...(groups.get(groupTitle) ?? []), article]);
  }

  return Array.from(groups.entries())
    .sort(([a], [b]) => compareGroupTitles(a, b))
    .map(([title, groupArticles]) => ({
      title,
      articles: [...groupArticles].sort(compareArticles),
    }));
}

function getArticleGroupTitle(description: string): string {
  const firstCharacter = description.trim().at(0);

  if (!firstCharacter) {
    return "#";
  }

  const normalized = firstCharacter
    .normalize("NFD")
    .replace(/\p{Mark}/gu, "")
    .toUpperCase();

  return /^[A-Z]$/.test(normalized) ? normalized : "#";
}

function compareGroupTitles(a: string, b: string): number {
  if (a === b) {
    return 0;
  }

  if (a === "#") {
    return -1;
  }

  if (b === "#") {
    return 1;
  }

  return a.localeCompare(b);
}

function compareArticles(a: ArticleListItemDto, b: ArticleListItemDto): number {
  return articleDescriptionCollator.compare(a.description, b.description)
    || a.id.localeCompare(b.id);
}
