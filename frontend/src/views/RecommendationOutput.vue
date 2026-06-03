<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getRecommendations } from '../api';

const router = useRouter();
const route = useRoute();
const recommendations = ref(null);
const loading = ref(true);
const error = ref('');
const playerId = route.params.playerId;

const contentType = computed(() => {
  if (!recommendations.value?.content) return 'Unknown';
  return recommendations.value.content.recommendationType;
});

const isSaferGambling = computed(() => {
  return contentType.value === 'SaferGambling';
});

const isBlocked = computed(() => {
  return recommendations.value?.blocked || false;
});

function scoreLabel(score) {
  return `${(score * 100).toFixed(0)}%`;
}

async function loadRecommendations() {
  loading.value = true;
  error.value = '';

  // Try to get from route state first
  if (router.currentRoute.value.state?.recommendations) {
    recommendations.value = router.currentRoute.value.state.recommendations;
    loading.value = false;
    return;
  }

  // Otherwise fetch from API
  try {
    recommendations.value = await getRecommendations(playerId, 3);
  } catch (err) {
    error.value = `Failed to load recommendations: ${err.message}`;
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadRecommendations();
});
</script>

<template>
  <section class="output-view">
    <router-link :to="`/players/${playerId}`" class="back-link">← Back to Player</router-link>

    <header class="output-header">
      <h1>Recommendations for {{ playerId }}</h1>
      <p class="output-subtitle">Decision output grouped by content, options, and audit rationale.</p>
    </header>

    <div v-if="error" class="error-alert">{{ error }}</div>

    <div v-if="loading" class="loading">Loading recommendations...</div>

    <div v-else-if="recommendations" class="output-layout">
      <section v-if="isBlocked" class="panel blocked-panel">
        <h2>Recommendations Blocked</h2>
        <p>{{ recommendations.blockReason }}</p>
      </section>

      <section v-if="recommendations.content && !isBlocked && isSaferGambling" class="panel safer-panel">
        <div class="panel-head">
          <h2>Safer Gambling Guidance</h2>
          <span class="pill safer-pill">{{ contentType }}</span>
        </div>
        <p class="panel-headline">{{ recommendations.content.headline }}</p>
        <p class="panel-copy">{{ recommendations.content.message }}</p>
        <div class="meta-list">
          <p><strong>Reason:</strong> {{ recommendations.content.reason }}</p>
          <p><strong>Safe to Show:</strong> {{ recommendations.content.safeToShow ? 'Yes' : 'No' }}</p>
        </div>
      </section>

      <section v-else-if="recommendations.content && !isBlocked" class="panel content-panel">
        <div class="panel-head">
          <h2>{{ recommendations.content.headline }}</h2>
          <span class="pill content-pill">{{ contentType }}</span>
        </div>
        <p class="panel-copy">{{ recommendations.content.message }}</p>
        <div class="meta-list">
          <p><strong>Reason:</strong> {{ recommendations.content.reason }}</p>
          <p><strong>Safe to Show:</strong> {{ recommendations.content.safeToShow ? 'Yes' : 'No' }}</p>
        </div>
      </section>

      <section v-if="recommendations.riskRecalculation" class="panel">
        <h3>Risk Recalculation</h3>
        <div class="risk-grid">
          <p><strong>Previous Risk:</strong> {{ recommendations.riskRecalculation.previousRiskLevel }}</p>
          <p><strong>Current Risk:</strong> {{ recommendations.riskRecalculation.currentRiskLevel }}</p>
          <p><strong>Risk Changed:</strong> {{ recommendations.riskRecalculation.riskChanged ? 'Yes' : 'No' }}</p>
          <p><strong>Safer Triggered:</strong> {{ recommendations.riskRecalculation.saferGamblingTriggered ? 'Yes' : 'No' }}</p>
        </div>
        <p v-if="recommendations.riskRecalculation.changeReason" class="trigger-reason">
          <strong>Trigger Reason:</strong> {{ recommendations.riskRecalculation.changeReason }}
        </p>
      </section>

      <section v-if="recommendations.allowedOptions.length > 0" class="panel">
        <h3>Available Options</h3>
        <div class="option-list">
          <article
            v-for="option in recommendations.allowedOptions"
            :key="option.optionType"
            class="option-card option-allowed"
          >
            <div>
              <p class="option-label">{{ option.label }}</p>
              <p class="option-type">{{ option.optionType }}</p>
            </div>
            <div class="score">
              <strong>{{ scoreLabel(option.score) }}</strong>
              <span>Score</span>
            </div>
          </article>
        </div>
      </section>

      <section v-if="recommendations.blockedOptions.length > 0" class="panel">
        <h3>Blocked Options</h3>
        <div class="option-list">
          <article
            v-for="option in recommendations.blockedOptions"
            :key="option.optionType"
            class="option-card option-blocked"
          >
            <div>
              <p class="option-label">{{ option.label }}</p>
              <p class="option-type">{{ option.optionType }}</p>
            </div>
            <div class="blocked-text">Blocked</div>
          </article>
        </div>
      </section>

      <section v-if="recommendations.audit.length > 0" class="panel">
        <h3>Decision Audit Trail</h3>
        <div class="audit-list">
          <article
            v-for="(item, idx) in recommendations.audit"
            :key="idx"
            class="audit-row"
          >
            <p class="audit-rule">{{ item.ruleId }}</p>
            <p class="audit-desc">{{ item.description }}</p>
            <div class="audit-impact">
              <span>Weight Impact</span>
              <strong>{{ item.weightImpact > 0 ? '+' : '' }}{{ item.weightImpact.toFixed(2) }}</strong>
            </div>
          </article>
        </div>
      </section>

      <footer class="page-footer">
        <router-link to="/" class="back-home">Back to Players</router-link>
      </footer>
    </div>
  </section>
</template>

<style scoped>
.output-view {
  padding: 16px 0 36px;
  text-align: left;
}

.output-header {
  margin-bottom: 16px;
}

.output-subtitle {
  margin: 6px 0 0;
  color: #6b7280;
  font-size: 16px;
}

.output-layout {
  display: grid;
  gap: 14px;
}

.panel {
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  background: #ffffff;
  padding: 14px;
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.05);
}

.panel h2,
.panel h3 {
  margin: 0 0 12px;
  line-height: 1.25;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 10px;
  margin-bottom: 8px;
}

.panel-headline {
  margin: 0 0 8px;
  font-size: 22px;
  font-weight: 700;
}

.panel-copy {
  margin: 0 0 10px;
  color: #1f2937;
  line-height: 1.5;
}

.back-link {
  display: inline-block;
  margin-bottom: 16px;
  font-weight: 600;
  color: #2563eb;
  text-decoration: none;
}

.back-link:hover {
  color: #1d4ed8;
}

.pill {
  border-radius: 999px;
  padding: 6px 10px;
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}

.safer-pill {
  background: #fde68a;
  color: #92400e;
}

.content-pill {
  background: #dbeafe;
  color: #1d4ed8;
}

.blocked-panel {
  background: #fefce8;
  border-color: #fcd34d;
}

.safer-panel {
  background: #fffbeb;
  border-color: #fcd34d;
}

.content-panel {
  background: #eff6ff;
  border-color: #bfdbfe;
}

.meta-list p {
  margin: 6px 0;
}

.risk-grid {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  gap: 8px;
}

.risk-grid p {
  margin: 0;
}

.trigger-reason {
  margin: 10px 0 0;
  color: #374151;
}

.option-list,
.audit-list {
  display: grid;
  gap: 10px;
}

.option-card {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.option-allowed {
  background: #f0fdf4;
  border-color: #bbf7d0;
}

.option-blocked {
  background: #fef2f2;
  border-color: #fecaca;
}

.option-label {
  margin: 0;
  font-weight: 700;
}

.option-type {
  margin: 4px 0 0;
  color: #6b7280;
  font-size: 13px;
}

.score {
  text-align: right;
}

.score strong {
  display: block;
  font-size: 24px;
  color: #15803d;
}

.score span {
  font-size: 11px;
  color: #6b7280;
}

.blocked-text {
  font-weight: 800;
  color: #b91c1c;
}

.audit-row {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 12px;
  background: #fafafa;
}

.audit-rule {
  margin: 0;
  font-weight: 700;
}

.audit-desc {
  margin: 6px 0 10px;
  color: #374151;
}

.audit-impact {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: #6b7280;
}

.audit-impact strong {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace;
  color: #111827;
}

.page-footer {
  text-align: center;
}

.back-home {
  color: #2563eb;
  font-weight: 700;
  text-decoration: none;
}

.back-home:hover {
  color: #1d4ed8;
}

@media (min-width: 900px) {
  .risk-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
