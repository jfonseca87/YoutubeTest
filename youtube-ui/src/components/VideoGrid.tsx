import { Button } from "primereact/button";
import { ProgressSpinner } from "primereact/progressspinner";
import { useTranslation } from "react-i18next";
import { useInfiniteVideos } from "../hooks/useInfiniteVideos";
import type { Video } from "../types/video";
import VideoCard from "./VideoCard";
import "./VideoGrid.css";

function videoKey(video: Video, index: number): string {
  const { id } = video;
  if (typeof id === "string" && id) return id;
  if (id && typeof id === "object" && id.videoId) return id.videoId;
  return `video-${index}`;
}

export default function VideoGrid() {
  const { t } = useTranslation();
  const { items, loading, loadingMore, error, hasMore, sentinelRef, retry } =
    useInfiniteVideos();

  if (loading) {
    return (
      <div
        className="video-grid__panel"
        role="status"
        aria-label={t("status.loading")}
      >
        <ProgressSpinner />
      </div>
    );
  }

  if (error === "initial") {
    return (
      <div className="video-grid__panel" role="alert">
        <p className="video-grid__message">{t("status.error")}</p>
        <Button label={t("status.retry")} onClick={retry} />
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="video-grid__panel">
        <p className="video-grid__message">{t("status.empty")}</p>
      </div>
    );
  }

  return (
    <div className="video-grid">
      {items.map((video, index) => (
        <VideoCard key={videoKey(video, index)} video={video} />
      ))}
      <div className="video-grid__footer">
        {loadingMore && (
          <div
            className="video-grid__inline-status"
            role="status"
            aria-label={t("status.loadingMore")}
          >
            <ProgressSpinner />
          </div>
        )}
        {!loadingMore && error === "more" && (
          <div className="video-grid__inline-error" role="alert">
            <span className="video-grid__message">{t("status.error")}</span>
            <Button label={t("status.retry")} onClick={retry} />
          </div>
        )}
        {!loadingMore && !error && !hasMore && (
          <p className="video-grid__end">{t("status.endOfList")}</p>
        )}
      </div>
      <div
        ref={sentinelRef}
        className="video-grid__sentinel"
        aria-hidden="true"
      />
    </div>
  );
}
