<script setup>
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getPlayer, getRecommendations } from '../api';

const router = useRouter();
const route = useRoute();
const player = ref(null);
const loading = ref(false);
const recommending = ref(false);
const error = ref('');
const playerId = route.params.playerId;

async function loadPlayer() {
  loading.value = true;
  error.value = '';
  try {
    player.value = await getPlayer(playerId);
  } catch (err) {
    error.value = `Failed to load player: ${err.message}`;
  } finally {
    loading.value = false;
  }
}

async function triggerRecommendations() {
  recommending.value = true;
  error.value = '';
  try {
    const recommendations = await getRecommendations(playerId, 3);
    router.push({
      name: 'RecommendationOutput',
      params: { playerId },
      state: { recommendations }
    });
  } catch (err) {
    error.value = `Failed to get recommendations: ${err.message}`;
  } finally {
    recommending.value = false;
  }
}

function normalizeRiskLevel(riskLevel) {
  return String(riskLevel || '').toLowerCase();
}

function riskBadgeClass(riskLevel) {
  const normalized = normalizeRiskLevel(riskLevel);

  if (normalized === 'low') return 'risk risk-low';
  if (normalized === 'medium') return 'risk risk-medium';
  if (normalized === 'high') return 'risk risk-high';
  if (normalized === 'very high') return 'risk risk-very-high';

  return 'risk risk-unknown';
}

function formatCurrency(value) {
  return new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: 'GBP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value || 0);
}

function formatPercent(value) {
  return `${((value || 0) * 100).toFixed(0)}%`;
}

onMounted(() => {
  loadPlayer();
});
</script>

<template>
  <section class="detail-view">
    <router-link to="/" class="back-link">
      ← Back to Players
    </router-link>

    <div v-if="error" class="error-alert">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">Loading player...</div>

    <article v-else-if="player" class="player-shell">
      <header class="player-header">
        <div>
          <p class="eyebrow">Player Profile</p>
          <h1>{{ player.playerId }}</h1>
        </div>
        <span :class="riskBadgeClass(player.riskLevel)">{{ player.riskLevel }}</span>
      </header>

      <div class="detail-grid">
        <div class="section-card">
          <h2>Betting Profile</h2>
          <dl class="kv-list">
            <div><dt>Primary Sport</dt><dd>{{ player.mostBetSport }}</dd></div>
            <div><dt>Favourite Team</dt><dd>{{ player.favouriteTeam }}</dd></div>
            <div><dt>Preferred Bet Type</dt><dd>{{ player.mostBetType }}</dd></div>
            <div><dt>Days Active</dt><dd>{{ player.daysSinceJoined }}</dd></div>
          </dl>
        </div>

        <div class="section-card">
          <h2>Risk & Compliance</h2>
          <dl class="kv-list">
            <div><dt>Risk Level</dt><dd>{{ player.riskLevel }}</dd></div>
            <div><dt>Self-Excluded</dt><dd>{{ player.isSelfExcluded ? 'Yes' : 'No' }}</dd></div>
            <div>
              <dt>Cooling-Off Until</dt>
              <dd>{{ player.coolingOffUntilUtc ? new Date(player.coolingOffUntilUtc).toLocaleDateString() : 'None' }}</dd>
            </div>
            <div><dt>Jurisdiction</dt><dd>{{ player.jurisdictionCode }}</dd></div>
          </dl>
        </div>
      </div>

      <div class="section-card metrics-section">
        <h2>Behaviour Metrics (30d)</h2>
        <div class="metric-grid">
          <div class="metric-tile">
            <span class="metric-label">Average Stake</span>
            <span class="metric-value">{{ formatCurrency(player.averageStake) }}</span>
          </div>
          <div class="metric-tile">
            <span class="metric-label">Stake Volatility</span>
            <span class="metric-value">{{ formatCurrency(player.stakeStdDev30d) }}</span>
          </div>
          <div class="metric-tile">
            <span class="metric-label">Sport Mix</span>
            <span class="metric-value">{{ formatPercent(player.sportMix30d) }}</span>
          </div>
          <div class="metric-tile">
            <span class="metric-label">Bet Type Mix</span>
            <span class="metric-value">{{ formatPercent(player.betTypeMix30d) }}</span>
          </div>
          <div class="metric-tile">
            <span class="metric-label">Last Login</span>
            <span class="metric-value">{{ player.lastLoginDaysAgo }} days ago</span>
          </div>
          <div class="metric-tile">
            <span class="metric-label">Time-of-Day Fit</span>
            <span class="metric-value">{{ formatPercent(player.timeOfDayFitScore) }}</span>
          </div>
        </div>
      </div>

      <button
        @click="triggerRecommendations"
        :disabled="recommending"
        class="recommend-btn"
      >
        {{ recommending ? 'Getting Recommendations...' : 'Get Recommendations' }}
      </button>
    </article>
  </section>
</template>

<style scoped>
.detail-view {
  padding: 12px 0 32px;
}

.back-link {
  display: inline-block;
  margin-bottom: 16px;
  color: #2563eb;
  font-weight: 600;
  text-decoration: none;
}

.back-link:hover {
  color: #1d4ed8;
}

.player-shell {
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  background: #ffffff;
  padding: 18px;
  display: grid;
  gap: 16px;
}

.player-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.eyebrow {
  margin: 0;
  color: #6b7280;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

h1 {
  margin: 4px 0 0;
}

.detail-grid {
  display: grid;
  gap: 12px;
}

.section-card {
  border: 1px solid #eef2f7;
  border-radius: 12px;
  padding: 14px;
  background: #fcfcfd;
}

.section-card h2 {
  margin: 0 0 10px;
  font-size: 18px;
}

.kv-list {
  display: grid;
  gap: 8px;
}

.kv-list div {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  border-bottom: 1px dashed #e5e7eb;
  padding-bottom: 6px;
}

.kv-list div:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.kv-list dt {
  color: #6b7280;
  font-size: 13px;
}

.kv-list dd {
  margin: 0;
  color: #111827;
  font-weight: 600;
  text-align: right;
}

.metric-grid {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  gap: 10px;
}

.metric-tile {
  border: 1px solid #eef2f7;
  border-radius: 10px;
  padding: 10px;
  background: #ffffff;
}

.metric-label {
  display: block;
  color: #6b7280;
  font-size: 12px;
  margin-bottom: 4px;
}

.metric-value {
  display: block;
  color: #111827;
  font-size: 18px;
  font-weight: 700;
}

.recommend-btn {
  width: 100%;
  border: 0;
  border-radius: 12px;
  padding: 12px 16px;
  background: #2563eb;
  color: #ffffff;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
}

.recommend-btn:hover {
  background: #1d4ed8;
}

.recommend-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.risk {
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  padding: 6px 10px;
  white-space: nowrap;
}

.risk-low {
  background: #dcfce7;
  color: #166534;
}

.risk-medium {
  background: #fef3c7;
  color: #92400e;
}

.risk-high,
.risk-very-high {
  background: #fee2e2;
  color: #991b1b;
}

.risk-unknown {
  background: #e5e7eb;
  color: #374151;
}

@media (min-width: 900px) {
  .detail-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .metric-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
</style>
