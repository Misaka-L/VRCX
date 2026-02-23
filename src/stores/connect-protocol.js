import { defineStore } from 'pinia';

import { useGameLogStore } from './gameLog';
import { useLocationStore } from './location';
import { useUserStore } from './user';

export const useConnectProtocolStore = defineStore('ConnectProtocol', () => {
    const gameLogStore = useGameLogStore();
    const locationStore = useLocationStore();
    const userStore = useUserStore();

    /**
     * @param {string} userId
     * @param {?string} tagColor
     */
    function onUserTagUpdated(userId, tagColor) {
        console.log('ConnectProtocolStore: onUserTagUpdated', userId, tagColor);
    }

    /**
     * @param {number} count
     */
    function onOnlineFriendCountUpdated(count) {
        console.log('ConnectProtocolStore: onOnlineFriendCountUpdated', count);
    }

    /**
     * @param {any[]} feedData
     */
    function onFeedUpdated(feedData) {
        console.log('ConnectProtocolStore: onFeedUpdated', feedData);
    }

    function onLocationUpdated() {
        const lastLocation = {
            date: locationStore.lastLocation.date,
            location: locationStore.lastLocation.location,
            name: locationStore.lastLocation.name,
            players: Array.from(locationStore.lastLocation.playerList.values()),
            friends: Array.from(locationStore.lastLocation.friendList.values()),
            onlineFor: userStore.currentUser.$online_for
        };

        console.log('ConnectProtocolStore: onLocationUpdated', lastLocation);
    }

    function onMediaUpdated() {
        console.log(
            'ConnectProtocolStore: onMediaUpdated',
            gameLogStore.nowPlaying
        );
    }

    /**
     * @param {object} noty
     * @param {string?} message
     * @param {string?} pathToLocalImage
     */
    function sendNotification(noty, message, pathToLocalImage) {
        console.log(
            'ConnectProtocolStore: sendNotification',
            noty,
            message,
            pathToLocalImage
        );
    }

    return {
        onUserTagUpdated,
        onOnlineFriendCountUpdated,
        onFeedUpdated,
        onMediaUpdated,
        onLocationUpdated,
        sendNotification
    };
});
