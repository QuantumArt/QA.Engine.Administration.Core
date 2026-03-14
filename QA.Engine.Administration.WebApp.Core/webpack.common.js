const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
    entry: {
        main: './ClientApp/index.tsx'
    },
    output: {
        filename: '[name].bundle.js',
        path: path.join(__dirname, 'wwwroot/dist'),
        publicPath: '/dist/'
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.json'],
        modules: ['node_modules'],
        alias: {
            components: path.resolve(__dirname, 'ClientApp/components/'),
            constants: path.resolve(__dirname, 'ClientApp/constants/'),
            enums: path.resolve(__dirname, 'ClientApp/enums/'),
            services: path.resolve(__dirname, 'ClientApp/services/'),
            stores: path.resolve(__dirname, 'ClientApp/stores/'),
            assets: path.resolve(__dirname, 'ClientApp/assets/'),
            qp: path.resolve(__dirname, 'ClientApp/qp/'),
        }
    },
    module: {
        rules: [
            {
                test: /\.(jpg|jpeg|png|gif|svg)?$/,
                exclude: /fonts/,
                type: 'asset',
                generator: { filename: 'img/[name][ext]' }
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf|svg)?$/,
                exclude: /img/,
                type: 'asset',
                generator: { filename: 'fonts/[name][ext]' }
            },
        ]
    },
    plugins: [
        new webpack.DefinePlugin({
            'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'production'),
            'process.env': JSON.stringify({}),
            'process': JSON.stringify({ env: { NODE_ENV: process.env.NODE_ENV || 'production' } }),
        }),
        new HtmlWebpackPlugin({
            title: 'Manage Site',
            template: 'ClientApp/assets/index.html'
        }),
        new CopyPlugin({
            patterns: [{
                from: './ClientApp/assets/pmrpc.js',
                to: './scripts'
            }]
        })
    ],
};
