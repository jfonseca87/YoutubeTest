import { useTranslation } from 'react-i18next';
import type { ThumbnailSet, Video } from '../types/video';
import './VideoCard.css';

const THUMBNAIL_PREFERENCE = [
  'maxres',
  'standard',
  'high',
  'medium',
  'default',
] as const;

function getBestThumbnail(
  thumbnails: ThumbnailSet | null | undefined,
): string | undefined {
  if (!thumbnails) return undefined;
  for (const key of THUMBNAIL_PREFERENCE) {
    const url = thumbnails[key]?.url;
    if (url) return url;
  }
  return undefined;
}

interface VideoCardProps {
  video: Video;
}

export default function VideoCard({ video }: VideoCardProps) {
  const { t } = useTranslation();
  const title = video.snippet?.title || t('video.noTitle');
  const channel = video.snippet?.channelTitle || t('video.unknownChannel');
  const thumbnailUrl = getBestThumbnail(video.snippet?.thumbnails);

  return (
    <article className="video-card">
      <div className="video-card__thumbnail">
        {thumbnailUrl ? (
          <img src={thumbnailUrl} alt={title} loading="lazy" />
        ) : (
          <div className="video-card__placeholder" aria-hidden="true">
            <i className="pi pi-play" />
          </div>
        )}
      </div>
      <div className="video-card__meta">
        <h3 className="video-card__title">{title}</h3>
        <p className="video-card__channel">{channel}</p>
      </div>
    </article>
  );
}
