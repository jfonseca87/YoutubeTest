import { useCallback, useEffect, useRef, useState, type RefObject } from 'react';
import { fetchVideos } from '../services/videoService';
import type { Video } from '../types/video';

const PAGE_SIZE = 24;

type LoadError = 'initial' | 'more';

interface UseInfiniteVideosResult {
  items: Video[];
  page: number;
  totalPages: number;
  loading: boolean;
  loadingMore: boolean;
  error: LoadError | null;
  hasMore: boolean;
  sentinelRef: RefObject<HTMLDivElement | null>;
  retry: () => void;
}

export function useInfiniteVideos(): UseInfiniteVideosResult {
  const [items, setItems] = useState<Video[]>([]);
  const [page, setPage] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<LoadError | null>(null);
  const [reloadToken, setReloadToken] = useState(0);
  const abortRef = useRef<AbortController | null>(null);
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    abortRef.current = controller;
    let active = true;

    setLoading(true);
    setError(null);
    setItems([]);
    setPage(0);
    setTotalPages(0);

    fetchVideos(1, PAGE_SIZE, controller.signal)
      .then((data) => {
        if (!active || controller.signal.aborted) return;
        setItems(data.items ?? []);
        setPage(data.page ?? 1);
        setTotalPages(data.totalPages ?? 0);
      })
      .catch(() => {
        if (!active || controller.signal.aborted) return;
        setError('initial');
      })
      .finally(() => {
        if (!active || controller.signal.aborted) return;
        setLoading(false);
      });

    return () => {
      active = false;
      controller.abort();
    };
  }, [reloadToken]);

  const loadMore = useCallback(
    async (options?: { ignoreError?: boolean }) => {
      if (loading || loadingMore) return;
      if (!options?.ignoreError && error) return;
      if (page >= totalPages) return;

      const controller = new AbortController();
      abortRef.current = controller;
      setLoadingMore(true);
      setError(null);

      try {
        const data = await fetchVideos(page + 1, PAGE_SIZE, controller.signal);
        if (controller.signal.aborted) return;
        setItems((prev) => [...prev, ...(data.items ?? [])]);
        setPage(data.page ?? page + 1);
        setTotalPages(data.totalPages ?? totalPages);
      } catch {
        if (controller.signal.aborted) return;
        setError('more');
      } finally {
        if (!controller.signal.aborted) setLoadingMore(false);
      }
    },
    [loading, loadingMore, error, page, totalPages],
  );

  const hasMore =
    !loading && !loadingMore && !error && items.length > 0 && page < totalPages;

  useEffect(() => {
    if (!hasMore) return;
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          void loadMore();
        }
      },
      { rootMargin: '300px' },
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasMore, loadMore]);

  const retry = useCallback(() => {
    if (error === 'more') {
      setError(null);
      void loadMore({ ignoreError: true });
    } else {
      setReloadToken((token) => token + 1);
    }
  }, [error, loadMore]);

  return {
    items,
    page,
    totalPages,
    loading,
    loadingMore,
    error,
    hasMore,
    sentinelRef,
    retry,
  };
}
