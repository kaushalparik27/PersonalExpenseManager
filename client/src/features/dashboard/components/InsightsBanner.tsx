import { Sparkles } from 'lucide-react'

interface InsightsBannerProps {
  insights: string[]
}

export function InsightsBanner({ insights }: InsightsBannerProps) {
  if (insights.length === 0 || (insights.length === 1 && insights[0] === 'Not enough data yet.')) {
    return null
  }

  return (
    <div className="flex flex-wrap items-center gap-x-5 gap-y-1.5 rounded-lg border border-border bg-accent/40 px-4 py-2.5">
      <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
        <Sparkles className="size-3.5 text-primary" />
        Insights
      </div>
      {insights.map((insight) => (
        <span key={insight} className="text-sm text-foreground">
          {insight}
        </span>
      ))}
    </div>
  )
}
