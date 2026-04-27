import { Link, useNavigate } from "react-router";
import { useState } from "react";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { ArticleForm } from "@cashregister/routes/articles/components/article-form";
import { deps } from "@cashregister/deps";
import type { ArticleDto } from "@cashregister/model";

interface ArticleDetailPanelProps {
  article: ArticleDto;
  closeTo: string;
}

export function ArticleDetailPanel({ article, closeTo }: ArticleDetailPanelProps) {
  const { addError } = useErrorMessages();
  const navigate = useNavigate();
  const [deleting, setDeleting] = useState(false);

  async function deleteArticle() {
    setDeleting(true);

    try {
      const result = await deps.apiClient.del(`/articles/${article.id}`);

      if (!result.ok) {
        addError(result.error.message);
        return;
      }

      navigate(closeTo);
    } finally {
      setDeleting(false);
    }
  }

  return (
    <>
      <header className="flex items-start justify-between gap-4 border-b p-4">
        <div className="min-w-0">
          <h2 className="text-xl font-semibold">{article.description}</h2>
          <div className="mt-3 space-y-2 text-sm text-gray-600">
            <div className="flex items-start justify-between gap-4">
              <span className="font-medium text-gray-700">Article ID</span>
              <span className="break-all text-right">{article.id}</span>
            </div>
          </div>
        </div>
        <Link
          to={closeTo}
          aria-label="Close article details"
          className="text-2xl leading-none text-gray-400 hover:text-gray-700"
        >
          ✕
        </Link>
      </header>
      <main className="flex-1 overflow-auto">
        <ArticleForm
          key={article.id}
          articleId={article.id}
          initialData={{
            description: article.description,
            priceInCents: article.priceInCents,
          }}
          onError={(message) => addError(message)}
        />
      </main>
      <footer className="border-t p-4">
        <button
          type="button"
          onClick={deleteArticle}
          disabled={deleting}
          className="w-full rounded border border-red-300 px-4 py-2 text-sm text-red-600 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {deleting ? "Deleting..." : "Delete"}
        </button>
      </footer>
    </>
  );
}
