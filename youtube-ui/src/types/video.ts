export interface Thumbnail {
  url?: string | null;
  width?: number | null;
  height?: number | null;
}

export interface ThumbnailSet {
  default?: Thumbnail | null;
  medium?: Thumbnail | null;
  high?: Thumbnail | null;
  standard?: Thumbnail | null;
  maxres?: Thumbnail | null;
}

export interface LocalizedText {
  title?: string | null;
  description?: string | null;
}

export interface VideoSnippet {
  kind?: string | null;
  etag?: string | null;
  publishedAt?: string | null;
  channelId?: string | null;
  title?: string | null;
  description?: string | null;
  thumbnails?: ThumbnailSet | null;
  channelTitle?: string | null;
  categoryId?: string | null;
  liveBroadcastContent?: string | null;
  localized?: LocalizedText | null;
  defaultAudioLanguage?: string | null;
}

export interface VideoContentDetails {
  duration?: string | null;
  dimension?: string | null;
  definition?: string | null;
  caption?: string | null;
  licensedContent?: boolean | null;
  contentRating?: Record<string, unknown> | null;
  projection?: string | null;
}

export interface VideoStatus {
  uploadStatus?: string | null;
  privacyStatus?: string | null;
  license?: string | null;
  embeddable?: boolean | null;
  publicStatsViewable?: boolean | null;
  madeForKids?: boolean | null;
  selfDeclaredMadeForKids?: boolean | null;
}

export interface VideoStatistics {
  viewCount?: string | null;
  likeCount?: string | null;
  favoriteCount?: string | null;
  commentCount?: string | null;
}

export interface VideoIdObject {
  kind?: string | null;
  videoId?: string | null;
}

export interface Video {
  kind?: string | null;
  etag?: string | null;
  id?: string | VideoIdObject | null;
  snippet?: VideoSnippet | null;
  contentDetails?: VideoContentDetails | null;
  status?: VideoStatus | null;
  statistics?: VideoStatistics | null;
}

export interface PagedVideosResponse {
  items?: Video[] | null;
  page?: number | null;
  pageSize?: number | null;
  totalCount?: number | null;
  totalPages?: number | null;
}
