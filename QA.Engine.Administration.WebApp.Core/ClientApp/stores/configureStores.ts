import UIStore from './UIStore';

export default function configureStores() {
    return {
        UIStore: new UIStore(),
    };
}
