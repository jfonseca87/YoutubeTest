import type { PagedVideosResponse } from "../types/video";

const API_BASE_URL: string =
  import.meta.env.VITE_API_URL ?? "https://localhost:7018";

export async function fetchVideos(
  page: number,
  pageSize: number,
  signal?: AbortSignal,
): Promise<PagedVideosResponse> {
  const url = new URL("/api/videos", API_BASE_URL);
  url.searchParams.set("page", String(page));
  url.searchParams.set("pageSize", String(pageSize));

  const response = await fetch(url.toString(), { signal });

  if (!response.ok) {
    throw new Error(
      `Failed to fetch videos: ${response.status} ${response.statusText}`,
    );
  }

  return (await response.json()) as PagedVideosResponse;
}
